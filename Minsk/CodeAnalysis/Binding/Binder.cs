using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;

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

            BoundScope scope = null;
            while (stack.Any())
            {
                globalScope = stack.Pop();

                scope = new BoundScope(scope);

                foreach (var variable in globalScope.Variables)
                    scope.TryDeclare(variable);
            }

            return scope;
        }

        private BoundStatement BindStatement(Statement node)
        {
            return node.Kind switch {
                SyntaxKind.BlockStatement
                    => BindBlockStatement(node as BlockStatement),

                SyntaxKind.ExpressionStatement
                    => BindExpressionStatement(node as ExpressionStatement),

                SyntaxKind.ConditionalStatement
                    => BindConditionalStatement(node as ConditionalStatement),

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
            var condition = BindExpression(node.Condition, typeof(bool));
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
            var expression = BindExpression(node.Expression);
            return new BoundExpressionStatement(expression);
        }

        private BoundStatement BindVariableDeclaration(
            VariableDeclarationStatement node)
        {
            var name = node.IdentifierToken.Text;
            var isReadOnly = node.KeywordToken.Kind == TokenKind.LetKeyword;
            var expression = BindExpression(node.Expression);
            var variable = new VariableSymbol(name, isReadOnly, expression.Type);

            if (!scope.TryDeclare(variable))
                diagnostics.Binding.VariableRedeclaration(node);

            return new BoundVariableDeclarationStatement(variable, expression);
        }

        private BoundStatement BindWhileStatement(WhileStatement node)
        {
            var condition = BindExpression(node.Condition, typeof(bool));
            var body = BindStatement(node.Body);
            return new BoundWhileStatement(condition, body);
        }

        public BoundExpression BindExpression(Expression node)
        {
            return node.Kind switch {
                SyntaxKind.AssignmentExpression
                    => BindAssignmentExpression(node as AssignmentExpression),

                SyntaxKind.BinaryExpression
                    => BindBinaryExpression(node as BinaryExpression),

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

        public BoundExpression BindExpression(Expression node, Type targetType)
        {
            var expression = BindExpression(node);

            if (expression.Type != targetType)
                diagnostics.Binding.CannotConvert(node, expression.Type, targetType);

            return expression;
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpression node)
        {
            var name = node.IdentifierToken.Text;
            var expression = BindExpression(node.Expression);

            var (success, variable) = scope.TryLookup(name);

            if (!success)
                diagnostics.Binding.UndeclaredIdentifier(node);
            else if (variable.IsReadOnly)
                diagnostics.Binding.CannotAssignToReadOnlyVariable(node);
            else if (expression.Type != variable.Type)
                diagnostics.Binding.CannotConvert(node, expression.Type, variable);

            return new BoundAssignmentExpression(variable, expression);
        }

        private BoundExpression BindBinaryExpression(BinaryExpression node)
        {
            var left = BindExpression(node.Left);
            var right = BindExpression(node.Right);

            var opToken = node.OperatorToken;

            var op = BoundBinaryOperator.Bind(
                opToken.Kind,
                left.Type,
                right.Type);

            if (op is null)
            {
                // ToDo: move into the call, and check whether left or right is a BoundNameExpression
                diagnostics.Binding.UndefinedOperator(
                    node,
                    opToken.Span,
                    $"Binary operator '{opToken.Kind}' is not defined for types {left.Type} and {right.Type}");

                return left;
            }

            return new BoundBinaryExpression(left, op, right);
        }

        private BoundLiteralExpression BindLiteralExpression(LiteralExpression node)
        {
            var value = node.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(NameExpression node)
        {
            var name = node.IdentifierToken.Text;

            var (found, variable) = scope.TryLookup(name);

            if (!found)
            {
                diagnostics.Binding.UndeclaredIdentifier(node);
                return new BoundVariableExpression(variable);
            }

            return new BoundVariableExpression(variable);
        }


        private BoundExpression BindParenthesizedExpression(ParenthesizedExpression node)
            => BindExpression(node.Expression);

        private BoundExpression BindUnaryExpression(UnaryExpression node)
        {
            var operand = BindExpression(node.Operand);

            var opToken = node.OperatorToken;

            var op = BoundUnaryOperator.Bind(
                opToken.Kind,
                operand.Type);

            if (op is null)
            {
                diagnostics.Binding.UndefinedOperator(
                    node,
                    opToken.Span,
                    $"Unary operator '{opToken.Kind}' is not defined for type {operand.Type}");

                return operand;
            }

            return new BoundUnaryExpression(op, operand);
        }
    }
}