using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag diagnostics;
        private BoundScope scope;

        public Binder(DiagnosticBag diagnostics, BoundScope parent)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            scope = new BoundScope(parent);
        }

        public static BoundGlobalScope BindGlobalScope(
            BoundGlobalScope previous,
            CompilationUnit compilationUnit,
            DiagnosticBag diagnostics)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(diagnostics, parentScope);
            var statement = binder.BindStatement(compilationUnit.Statement);
            var variables = binder.scope.DeclaredVariables;

            return new BoundGlobalScope(previous, diagnostics, variables, statement);
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

        private BoundStatement BindStatement(Statement node)
        {
            return node.Kind switch {
                SyntaxKind.BlockStatement
                    => BindBlockStatement(node as BlockStatement),

                SyntaxKind.ConditionalStatement
                    => BindConditionalStatement(node as ConditionalStatement),

                SyntaxKind.ExpressionStatement
                    => BindExpressionStatement(node as ExpressionStatement),

                SyntaxKind.ForToStatement
                    => BindForToStatement(node as ForToStatement),

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

        private BoundStatement BindConditionalStatement(ConditionalStatement node)
        {
            var condition = BindExpression(node.Condition, TypeSymbol.Bool);
            var statement = BindStatement(node.ThenStatement);
            var elseStatement = BindOptionalElseClause(node.ElseClause);

            return new BoundConditionalStatement(condition, statement, elseStatement);
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
            var body = BindStatement(node.Body);
            scope = scope.Parent;

            return new BoundForToStatement(variable, lowerBound, upperBound, body);
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationStatement node)
        {
            var isReadOnly = node.KeywordToken.Kind == TokenKind.LetKeyword;
            var type = BindTypeClause(node.OptionalTypeClause);
            var expression = BindExpression(node.Expression);

            var variableType = type ?? expression.Type;
            var convertedInitializer = BindConversion(node.Expression.Span, node, expression, variableType);

            var variable = BindVariable(node, node.Identifier, variableType, isReadOnly);

            return new BoundVariableDeclarationStatement(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax node)
        {
            if (node is null)
                return null;

            var type = LookupType(node.Identifier.Text);

            if (type is null)
            {
                Report.UndefinedType(node, node.Identifier);
                return TypeSymbol.Error;
            }

            return type;
        }

        private BoundStatement BindWhileStatement(WhileStatement node)
        {
            var condition = BindExpression(node.Condition, TypeSymbol.Bool);
            var body = BindStatement(node.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundExpression BindExpression(Expression node, TypeSymbol targetType)
        {
            var expression = BindExpression(node);

            if (!targetType.IsErrorType && !expression.Type.IsErrorType && expression.Type != targetType)
                Report.CannotConvert(node, node.Span, expression.Type, targetType);

            return expression;
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
            var name = node.IdentifierToken.Text;
            var expression = BindExpression(node.Expression);

            if (expression is BoundErrorExpression)
                return new BoundErrorExpression();

            var (success, variable) = scope.TryLookupVariable(name);

            if (!success)
                Report.UndeclaredIdentifier(node);
            else if (variable.IsReadOnly)
                Report.CannotAssignToReadOnlyVariable(node);

            var convertedExpression = BindConversion(node.Span, node, expression, variable.Type);

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
                Report.UndefinedOperator(
                    node,
                    opToken.Span,
                    $"Binary operator '{opToken.Kind}' is not defined for types {left.Type} and {right.Type}");

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

            var (success, function) = scope.TryLookupFunction(node.Identifier.Text);
            if (!success)
            {
                Report.UndefinedFunction(node);
                return new BoundErrorExpression();
            }

            if (node.Arguments.Count != function.Parameters.Length)
            {
                Report.MismatchingArgumentCount(node, function);
                return new BoundErrorExpression();
            }

            var hasError = false;

            for (var i = 0; i < function.Parameters.Length; i++)
            {
                var parameter = function.Parameters[i];
                var argument = boundArguments[i];

                if (parameter.Type != argument.Type && argument.Type.IsNotErrorType)
                {
                    var convertedArgument = BindConversion(node.Arguments[i].Span, node.Arguments[i], argument, parameter.Type);

                    if (convertedArgument.Type == parameter.Type)
                    {
                        boundArguments[i] = convertedArgument;
                    }
                    else
                    {
                        hasError = true;
                        Report.ArgumentTypeMismatch(node.Arguments[i], argument.Type, parameter);
                    }
                }
            }

            if (hasError)
                return new BoundErrorExpression();

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

            var (found, variable) = scope.TryLookupVariable(name);

            if (!found)
            {
                Report.UndeclaredIdentifier(node);
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
                Report.UndefinedOperator(
                    node,
                    opToken.Span,
                    $"Unary operator '{opToken.Kind}' is not defined for type {operand.Type}");

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
            return BindConversion(node.Span, node, expression, toType, allowExplicit);
        }

        private BoundExpression BindConversion(
            TextSpan span,
            SyntaxNode node,
            BoundExpression expression,
            TypeSymbol toType,
            bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, toType);

            if (conversion.IsIdentity)
                return expression;

            if (conversion.DoesNotExist)
            {
                if (expression.Type.IsNotErrorType && toType.IsNotErrorType)
                    Report.CannotConvert(node, span, expression.Type, toType);

                return new BoundErrorExpression();
            }

            if (conversion.IsExplicit && !allowExplicit)
            {
                Report.CannotImplicitlyConvert(node, span, expression.Type, toType);
                return new BoundErrorExpression();
            }

            return new BoundConversionExpression(expression, toType);
        }

        private VariableSymbol BindVariable(
            SyntaxNode node,
            LexToken identifier,
            TypeSymbol type,
            bool isReadOnly = false)
        {
            var isMissing = identifier.IsMissing;
            var name = identifier.Text ?? "?";

            var variable = new VariableSymbol(name, isReadOnly, type);

            if (!isMissing && !scope.TryDeclareVariable(variable))
                Report.VariableRedeclaration(node, identifier);

            return variable;
        }

        private TypeSymbol LookupType(string name)
        {
            return name switch
            {
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