using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Diagnostics;
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

        private BoundStatement BindStatement(Statement statement)
        {
            return statement.Kind switch {
                SyntaxKind.BlockStatement
                    => BindBlockStatement(statement as BlockStatement),

                SyntaxKind.ExpressionStatement
                    => BindExpressionStatement(statement as ExpressionStatement),

                _   => throw new NotImplementedException($"statement.Kind")
            };
        }

        private BoundStatement BindBlockStatement(BlockStatement blockStatement)
        {
            var boundStatements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (var statement in blockStatement.Statements)
                boundStatements.Add(BindStatement(statement));

            return new BoundBlockStatement(boundStatements.ToImmutable());
        }

        private BoundStatement BindExpressionStatement(ExpressionStatement expressionStatement)
        {
            var expression = BindExpression(expressionStatement.Expression);
            return new BoundExpressionStatement(expression);
        }

        public BoundExpression BindExpression(Expression expression)
        {
            return expression.Kind switch {
                SyntaxKind.AssignmentExpression
                    => BindAssignmentExpression(expression as AssignmentExpression),

                SyntaxKind.BinaryExpression
                    => BindBinaryExpression(expression as BinaryExpression),

                SyntaxKind.LiteralExpression
                    => BindLiteralExpression(expression as LiteralExpression),

                SyntaxKind.NameExpression
                    => BindNameExpression(expression as NameExpression),

                SyntaxKind.ParenthesesExpression
                    => BindParenthesizedExpression(expression as ParenthesizedExpression),

                SyntaxKind.UnaryExpression
                    => BindUnaryExpression(expression as UnaryExpression),

                _   => throw new NotImplementedException($"Unexpected syntax node '{expression.Kind}'")
            };
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpression assignment)
        {
            var name = assignment.IdentifierToken.Text;
            var expression = BindExpression(assignment.Expression);

            var (success, variable) = scope.TryLookup(name);

            if (!success)
            {
                variable = new VariableSymbol(name, expression.Type);
                success = scope.TryDeclare(variable);
            }
            else if (expression.Type != variable.Type)
            {
                diagnostics.Binding.CannotConvert(assignment, expression.Type, variable);
            }

            //if (!success)
            //    diagnostics.Binding.VariableRedeclaration(assignment);

            return new BoundAssignmentExpression(variable, expression);
        }

        private BoundExpression BindBinaryExpression(BinaryExpression binaryExpression)
        {
            var left = BindExpression(binaryExpression.Left);
            var right = BindExpression(binaryExpression.Right);

            var opToken = binaryExpression.OperatorToken;

            var op = BoundBinaryOperator.Bind(
                opToken.Kind,
                left.Type,
                right.Type);

            if (op is null)
            {
                // ToDo: move into the call, and check whether left or right is a BoundNameExpression
                diagnostics.Binding.UndefinedOperator(
                    binaryExpression,
                    opToken.Span,
                    $"Binary operator '{opToken.Kind}' is not defined for types {left.Type} and {right.Type}");

                return left;
            }

            return new BoundBinaryExpression(left, op, right);
        }

        private BoundLiteralExpression BindLiteralExpression(LiteralExpression literalExpression)
        {
            var value = literalExpression.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(NameExpression nameExpression)
        {
            var name = nameExpression.IdentifierToken.Text;

            var (found, variable) = scope.TryLookup(name);

            if (!found)
            {
                diagnostics.Binding.UndefinedIdentifier(nameExpression);
                return new BoundVariableExpression(variable);
            }

            return new BoundVariableExpression(variable);
        }


        private BoundExpression BindParenthesizedExpression(ParenthesizedExpression parenthesizedExpression)
            => BindExpression(parenthesizedExpression.Expression);

        private BoundExpression BindUnaryExpression(UnaryExpression unaryExpression)
        {
            var operand = BindExpression(unaryExpression.Operand);

            var opToken = unaryExpression.OperatorToken;

            var op = BoundUnaryOperator.Bind(
                opToken.Kind,
                operand.Type);

            if (op is null)
            {
                diagnostics.Binding.UndefinedOperator(
                    unaryExpression,
                    opToken.Span,
                    $"Unary operator '{opToken.Kind}' is not defined for type {operand.Type}");

                return operand;
            }

            return new BoundUnaryExpression(op, operand);
        }
    }
}