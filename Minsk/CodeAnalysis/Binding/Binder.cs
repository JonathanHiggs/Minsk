using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Lowering;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag diagnostics;
        private readonly FunctionSymbol function;
        private readonly bool isScript;
        private BoundScope scope;
        private int labelCounter = 1;
        private Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> loopStack
            = new Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)>();

        public Binder(
            DiagnosticBag diagnostics,
            BoundScope parent,
            FunctionSymbol function,
            bool isScript)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            this.function = function;
            this.isScript = isScript;

            scope = new BoundScope(parent);

            if (function is not null)
            {
                foreach (var p in function.Parameters)
                    scope.TryDeclareVariable(p);
            }
        }

        public static BoundGlobalScope BindGlobalScope(
            bool isScript,
            BoundGlobalScope previous,
            SyntaxTree syntaxTree
        )
            => BindGlobalScope(
                isScript,
                previous,
                new SyntaxTree[] { syntaxTree },
                syntaxTree.Diagnostics);

        public static BoundGlobalScope BindGlobalScope(
            bool isScript,
            BoundGlobalScope previous,
            IEnumerable<SyntaxTree> syntaxTrees,
            DiagnosticBag diagnostics)
        {
            var Report = diagnostics.Binding;   // Odour remover

            var parentScope = CreateParentScope(previous);
            var binder = new Binder(diagnostics, parentScope, function: null, isScript);

            var functionDeclarations =
                syntaxTrees.SelectMany(t => t.Root.Members)
                           .OfType<FunctionDeclaration>();

            foreach (var function in functionDeclarations)
                binder.BindFunctionDeclaration(function);

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            var globalStatements =
                syntaxTrees.SelectMany(t => t.Root.Members)
                           .OfType<GlobalStatementSyntax>();

            foreach (var globalStatement in globalStatements)
            {
                var statement = binder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            var functions = binder.scope.Functions.ToImmutableArray();
            var variables = binder.scope.Variables.ToImmutableArray();

            FunctionSymbol mainFunction = null;
            FunctionSymbol scriptFunction = null;

            if (isScript)
            {
                if (globalStatements.Any())
                    scriptFunction = "$eval".ReturnsAny();
            }
            else
            {
                var mainFunctions = functions.Where(f => f.Name == "Main").ToImmutableArray();
                if (mainFunctions.Length > 1)
                {
                    foreach (var main in mainFunctions)
                        Report.MultipleMainFunctions(main.Declaration);
                }

                static GlobalStatementSyntax FirstGlobalStatement(SyntaxTree tree)
                    => tree.Root.Members.OfType<GlobalStatementSyntax>().FirstOrDefault();

                if (mainFunctions.Length > 0 && globalStatements.Any())
                {
                    var firstGlobalInEachTree =
                        syntaxTrees.Select(FirstGlobalStatement).Where(s => s is not null);

                    foreach (var main in mainFunctions)
                        Report.CannotMixMainAndGlobalStatements(main.Declaration);

                    foreach (var statement in firstGlobalInEachTree)
                        Report.CannotMixMainAndGlobalStatements(statement);
                }

                if (mainFunctions.Length == 0 && globalStatements.Any())
                {
                    static (SyntaxTree SyntaxTree, GlobalStatementSyntax GlobalStatement) Pair(SyntaxTree tree)
                        => (tree, FirstGlobalStatement(tree));

                    var syntaxTreesWithGlobalStatement =
                        syntaxTrees.Select(Pair).Where(kv => kv.GlobalStatement is not null);

                    if (syntaxTreesWithGlobalStatement.Count() > 1)
                    {
                        foreach (var pair in syntaxTreesWithGlobalStatement)
                            Report.MultipleGlobalStatementFiles(pair.GlobalStatement);
                    }
                    else
                    {
                        mainFunction = new FunctionSymbol(
                            "main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void, null);
                    }
                }

                if (mainFunctions.Length == 1)
                {
                    mainFunction = mainFunctions[0];

                    if (mainFunction.ReturnType.IsNotVoidType)
                        Report.MainMustBeVoid(mainFunction.Declaration);

                    if (mainFunction.Parameters.Any())
                        Report.MainCannotHaveParameters(mainFunction.Declaration);
                }
                else
                {

                }
            }

            return new BoundGlobalScope(previous, diagnostics, mainFunction, scriptFunction, functions, variables, statements);
        }

        public static BoundProgram BindProgram(
            bool isScript,
            BoundProgram previousProgram,
            BoundGlobalScope globalScope,
            DiagnosticBag diagnostics)
        {
            var parentScope = CreateParentScope(globalScope);

            if (globalScope.Diagnostics.Any())
                return new BoundProgram(
                    globalScope.Diagnostics,
                    previousProgram,
                    globalScope.MainFunction,
                    globalScope.ScriptFunction,
                    ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Empty);

            var functionBodies
                = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();

            foreach(var function in globalScope.Functions)
            {
                var binder = new Binder(diagnostics, parentScope, function, isScript);
                var body = binder.BindStatement(function.Declaration.Body);
                var loweredBody = Lowerer.Lower(body);

                if (function.ReturnType.IsNotVoidType && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    binder.Report.AllPathsMustReturn(function);

                functionBodies.Add(function, loweredBody);
            }

            BoundBlockStatement statement;
            var statements = globalScope.Statements;

            if (globalScope.MainFunction is not null && globalScope.Statements.Any())
            {
                statement = Lowerer.Lower(new BoundBlockStatement(statements));
                functionBodies.Add(globalScope.MainFunction, statement);
            }
            else if (globalScope.ScriptFunction is not null)
            {
                if (statements.Length == 1
                    && statements[0] is BoundExpressionStatement expressionStatement
                    && expressionStatement.Expression.Type.IsNotVoidType)
                {
                    statements =
                        statements.SetItem(0, new BoundReturnStatement(expressionStatement.Expression));
                }
                else
                {
                    var nullValue = new BoundLiteralExpression("");
                    statements = statements.Add(new BoundReturnStatement(nullValue));
                }

                statement = Lowerer.Lower(new BoundBlockStatement(statements));
                functionBodies.Add(globalScope.ScriptFunction, statement);
            }

            return new BoundProgram(
                diagnostics,
                previousProgram,
                globalScope.MainFunction,
                globalScope.ScriptFunction,
                functionBodies.ToImmutable());
        }

        private void BindFunctionDeclaration(FunctionDeclaration node)
        {
            var name = node.Identifier.Text;
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();

            var seenParameterNames = new HashSet<string>();

            foreach (var parameterSyntax in node.Parameters)
            {
                var parameterName = parameterSyntax.Identifier.Text;
                var parameterType = BindTypeClause(parameterSyntax.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    Report.ParameterAlreadyDeclared(parameterSyntax);
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            var returnType = BindTypeClause(node.OptionalTypeClause) ?? TypeSymbol.Void;

            var function = new FunctionSymbol(name, parameters.ToImmutable(), returnType, node);
            if (!scope.TryDeclareFunction(function))
                Report.SymbolAlreadyDeclared(node);
        }

        private static BoundScope CreateParentScope(BoundGlobalScope globalScope)
        {
            var stack = new Stack<BoundGlobalScope>();

            while (globalScope is not null)
            {
                stack.Push(globalScope);
                globalScope = globalScope.Previous;
            }

            var scope = CreateRootScope();
            while (stack.Any())
            {
                globalScope = stack.Pop();
                scope = new BoundScope(scope);

                foreach (var function in globalScope.Functions)
                    scope.TryDeclareFunction(function);

                foreach (var variable in globalScope.Variables)
                    scope.TryDeclareVariable(variable);
            }

            return scope;
        }

        private static BoundScope CreateRootScope()
        {
            var scope = new BoundScope(null);

            foreach (var function in BuiltinFunctions.All)
                scope.TryDeclareFunction(function);

            return scope;
        }

        private BoundStatement BindGlobalStatement(Statement node)
            => BindStatement(node, true);

        private BoundStatement BindStatement(Statement node, bool isGlobal = false)
        {
            var result = BindStatementInternal(node);

            if (!isScript || !isGlobal)
            {
                if (result is BoundExpressionStatement es)
                {
                    var isAllowedExpression
                        = es.Expression.Kind == BoundNodeKind.ErrorExpression
                        || es.Expression.Kind == BoundNodeKind.AssignmentExpression
                        || es.Expression.Kind == BoundNodeKind.CallExpression;

                    if (!isAllowedExpression)
                        Report.InvalidExpressionStatement(node);
                }
            }

            return result;
        }

        private BoundStatement BindStatementInternal(Statement node)
        {
            return node.Kind switch {
                SyntaxKind.BlockStatement
                    => BindBlockStatement(node as BlockStatement),

                SyntaxKind.BreakStatement
                    => BindBreakStatement(node as BreakStatement),

                SyntaxKind.ConditionalStatement
                    => BindConditionalStatement(node as ConditionalStatement),

                SyntaxKind.ContinueStatement
                    => BindContinueStatement(node as ContinueStatement),

                SyntaxKind.ExpressionStatement
                    => BindExpressionStatement(node as ExpressionStatement),

                SyntaxKind.ForToStatement
                    => BindForToStatement(node as ForToStatement),

                SyntaxKind.ReturnStatement
                    => BindReturnStatement(node as ReturnStatement),

                SyntaxKind.VariableDeclaration
                    => BindVariableDeclaration(node as VariableDeclarationStatement),

                SyntaxKind.WhileStatement
                    => BindWhileStatement(node as WhileStatement),

                _   => throw new NotImplementedException($"statement.Kind")
            };
        }

        private BoundStatement BindBlockStatement(BlockStatement node)
        {
            var boundStatements = ImmutableArray.CreateBuilder<BoundStatement>();
            scope = new BoundScope(scope);

            foreach (var statement in node.Statements)
                boundStatements.Add(BindStatement(statement));

            scope = scope.Parent;
            return new BoundBlockStatement(boundStatements.ToImmutable());
        }

        private BoundStatement BindBreakStatement(BreakStatement node)
        {
            if (loopStack.Count == 0)
            {
                Report.InvalidBreak(node);
                return new BoundErrorStatement();
            }

            var currentLoop = loopStack.Peek();
            return new BoundGotoStatement(currentLoop.BreakLabel);
        }

        private BoundStatement BindConditionalStatement(ConditionalStatement node)
        {
            var condition = BindExpression(node.Condition, TypeSymbol.Bool);
            var statement = BindStatement(node.ThenStatement);
            var elseStatement = BindOptionalElseClause(node.ElseClause);

            return new BoundConditionalStatement(condition, statement, elseStatement);
        }

        private BoundStatement BindContinueStatement(ContinueStatement node)
        {
            if (loopStack.Count == 0)
            {
                Report.InvalidContinue(node);
                return new BoundErrorStatement();
            }

            var currentLoop = loopStack.Peek();
            return new BoundGotoStatement(currentLoop.ContinueLabel);
        }

        private BoundStatement BindOptionalElseClause(ElseClauseSyntax node)
        {
            if (node is null)
                return null;

            return BindStatement(node.Statement);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatement node)
        {
            var expression = BindExpression(node.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundStatement BindForToStatement(ForToStatement node)
        {
            var lowerBound = BindExpression(node.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(node.UpperBound, TypeSymbol.Int);

            scope = new BoundScope(scope);

            var variable = BindVariable(node, node.Identifier, TypeSymbol.Int, true);
            var (body, breakLabel, continueLabel) = BindLoopBody(node.Body);
            scope = scope.Parent;

            return new BoundForToStatement(
                variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }

        private BoundStatement BindReturnStatement(ReturnStatement node)
        {
            var expression = node.OptionalExpression is not null ? BindExpression(node.OptionalExpression) : null;

            if (function is null)
            {
                if (isScript)
                {
                    if (expression is null)
                        expression = new BoundLiteralExpression("");
                }
                else if (expression is not null)
                {
                    Report.InvalidReturnStatement(node);
                }
            }
            else
            {
                if (function.ReturnType.IsVoidType)
                {
                    if (expression is not null)
                        Report.InvalidReturnType(node, function.ReturnType, expression.Type);
                }
                else
                {
                    if (expression is null)
                        Report.MissingReturnExpression(node);
                    else
                        expression = BindConversion(node.OptionalExpression, function.ReturnType);
                }
            }

            return new BoundReturnStatement(expression);
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationStatement node)
        {
            var isReadOnly = node.KeywordToken.Kind == TokenKind.LetKeyword;
            var type = BindTypeClause(node.OptionalTypeClause);
            var initializer = BindExpression(node.Initializer);

            var variableType = type ?? initializer.Type;
            var variable = BindVariable(node, node.Identifier, variableType, isReadOnly);

            var convertedInitializer = BindConversion(node.Initializer.Location, node, initializer, variableType);

            return new BoundVariableDeclarationStatement(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax node)
        {
            if (node is null)
                return null;

            var type = LookupType(node.Identifier.Text);

            if (type is null)
                Report.UndefinedType(node, node.Identifier);

            return type;
        }

        private BoundStatement BindWhileStatement(WhileStatement node)
        {
            var condition = BindExpression(node.Condition, TypeSymbol.Bool);
            var (body, breakLabel, continueLabel) = BindLoopBody(node.Body);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        private (BoundStatement Statement, BoundLabel BreakLabel, BoundLabel ContinueLabel)
            BindLoopBody(Statement node)
        {
            var breakLabel = new BoundLabel($"break-{labelCounter++}");
            var continueLabel = new BoundLabel($"continue-{labelCounter++}");

            loopStack.Push((breakLabel, continueLabel));
            var boundBody = BindStatement(node);
            loopStack.Pop();

            return (boundBody, breakLabel, continueLabel);
        }

        private BoundExpression BindExpression(Expression node, TypeSymbol targetType)
        {
            return BindConversion(node, targetType);
        }

        private BoundExpression BindExpression(Expression node, bool canBeVoid = false)
        {
            var expression = BindExpressionInternal(node);

            if (!canBeVoid && expression.Type.IsVoidType)
            {
                Report.VoidExpression(node);
                return new BoundErrorExpression();
            }

            return expression;
        }

        private BoundExpression BindExpressionInternal(Expression node)
        {
            return node.Kind switch {
                SyntaxKind.AssignmentExpression
                    => BindAssignmentExpression(node as AssignmentExpression),

                SyntaxKind.BinaryExpression
                    => BindBinaryExpression(node as BinaryExpression),

                SyntaxKind.CallExpression
                    => BindCallExpression(node as CallExpression),

                SyntaxKind.LiteralExpression
                    => BindLiteralExpression(node as LiteralExpression),

                SyntaxKind.NameExpression
                    => BindNameExpression(node as NameExpression),

                SyntaxKind.ParenthesesExpression
                    => BindParenthesizedExpression(node as ParenthesizedExpression),

                SyntaxKind.UnaryExpression
                    => BindUnaryExpression(node as UnaryExpression),

                _   => throw new NotImplementedException($"Unexpected syntax node '{node.Kind}'")
            };
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpression node)
        {
            var name = node.Identifier.Text;
            var expression = BindExpression(node.Expression);

            if (expression is BoundErrorExpression)
                return new BoundErrorExpression();

            var (success, symbol) = scope.TryLookupSymbol(name);
            if (!success)
            {
                Report.UndeclaredIdentifier(node);
                return new BoundErrorExpression();
            }

            var variable = symbol as VariableSymbol;
            if (variable is null)
            {
                Report.NotAVariable(node, node.Identifier.Location, symbol);
                return new BoundErrorExpression();
            }

            if (variable.IsReadOnly)
                Report.CannotAssignToReadOnlyVariable(node);

            var type = variable?.Type ?? TypeSymbol.Error;

            var convertedExpression = BindConversion(node.Location, node, expression, type);

            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindBinaryExpression(BinaryExpression node)
        {
            var left = BindExpression(node.Left);
            var right = BindExpression(node.Right);

            if (left.Type == TypeSymbol.Error || right.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var opToken = node.OperatorToken;

            var op = BoundBinaryOperator.Bind(
                opToken.Kind,
                left.Type,
                right.Type);

            if (op is null)
            {
                // ToDo: move into the call, and check whether left or right is a BoundNameExpression
                Report.UndefinedOperator(node, opToken, left.Type, right.Type);

                return new BoundErrorExpression();
            }

            return new BoundBinaryExpression(left, op, right);
        }

        private BoundExpression BindCallExpression(CallExpression node)
        {
            var lookupType = LookupType(node.Identifier.Text);
            if (node.Arguments.Count == 1 && lookupType is not null && lookupType.IsNotVoidType)
                return BindConversion(node.Arguments[0], lookupType, allowExplicit: true);

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var argument in node.Arguments)
                boundArguments.Add(BindExpression(argument));

            var (success, symbol) = scope.TryLookupSymbol(node.Identifier.Text);
            if (!success)
            {
                Report.UndefinedFunction(node);
                return new BoundErrorExpression();
            }

            var function = symbol as FunctionSymbol;
            if (function is null)
            {
                Report.NotAFunction(node, symbol);
                return new BoundErrorExpression();
            }

            if (node.Arguments.Count != function.Parameters.Length)
            {
                Report.MismatchingArgumentCount(node, function);
                return new BoundErrorExpression();
            }

            for (var i = 0; i < function.Parameters.Length; i++)
            {
                var parameter = function.Parameters[i];
                var argument = boundArguments[i];
                var argumentNode = node.Arguments[i];
                var argumentLocation = argumentNode.Location;

                boundArguments[i] = BindConversion(argumentLocation, argumentNode, argument, parameter.Type);
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindLiteralExpression(LiteralExpression node)
        {
            if (node.FirstToken.IsMissing)
                return new BoundErrorExpression();

            var value = node.Value;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(NameExpression node)
        {
            var name = node.IdentifierToken.Text;

            // Happens when Identifier token is inserted by parser. Error should already be reported
            if (string.IsNullOrEmpty(name))
                return new BoundErrorExpression();

            var (success, symbol) = scope.TryLookupSymbol(name);
            if (!success)
            {
                Report.UndeclaredIdentifier(node);
                return new BoundErrorExpression();
            }

            var variable = symbol as VariableSymbol;
            if (variable is null)
            {
                Report.NotAVariable(node, node.IdentifierToken.Location, symbol);
                return new BoundErrorExpression();
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpression node)
            => BindExpression(node.Expression);

        private BoundExpression BindUnaryExpression(UnaryExpression node)
        {
            var operand = BindExpression(node.Operand);

            if (operand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var opToken = node.OperatorToken;

            var op = BoundUnaryOperator.Bind(
                opToken.Kind,
                operand.Type);

            if (op is null)
            {
                Report.UndefinedOperator(node, opToken, operand.Type);

                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression(op, operand);
        }

        private BoundExpression BindConversion(
            Expression node,
            TypeSymbol toType,
            bool allowExplicit = false)
        {
            var expression = BindExpression(node);
            return BindConversion(node.Location, node, expression, toType, allowExplicit);
        }

        private BoundExpression BindConversion(
            TextLocation location,
            SyntaxNode node,
            BoundExpression expression,
            TypeSymbol toType,
            bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, toType);

            if (conversion.DoesNotExist)
            {
                if (expression.Type.IsNotErrorType && toType.IsNotErrorType)
                    Report.CannotConvert(node, location, expression.Type, toType);

                return new BoundErrorExpression();
            }

            if (conversion.IsExplicit && !allowExplicit)
                Report.CannotImplicitlyConvert(node, location, expression.Type, toType);

            if (conversion.IsIdentity)
                return expression;

            return new BoundConversionExpression(expression, toType);
        }

        private VariableSymbol BindVariable(
            SyntaxNode node,
            LexToken identifier,
            TypeSymbol type,
            bool isReadOnly = false)
        {
            var name = identifier.Text ?? "?";
            var isMissing = identifier.IsMissing;

            var variable = function is null
                ? (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type)
                : (VariableSymbol)new LocalVariableSymbol(name, isReadOnly, type);

            if (!isMissing && !scope.TryDeclareVariable(variable))
                Report.VariableRedeclaration(node, identifier);

            return variable;
        }

        private TypeSymbol LookupType(string name)
        {
            return name switch
            {
                "any"       => TypeSymbol.Any,
                "bool"      => TypeSymbol.Bool,
                "int"       => TypeSymbol.Int,
                "string"    => TypeSymbol.String,
                "void"      => TypeSymbol.Void,
                _           => null
            };
        }


        private BindingDiagnostics Report => diagnostics.Binding;
    }
}