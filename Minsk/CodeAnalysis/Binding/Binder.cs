using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag diagnostics;
        private readonly Dictionary<string, object> variables;

        public Binder(DiagnosticBag diagnostics, Dictionary<string, object> variables)
        {
            this.diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            this.variables = variables
                ?? throw new ArgumentNullException(nameof(variables));
        }

        public BoundExpression BindExpression(Expression expression)
        {
            switch (expression.Kind)
            {
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression(expression as AssignmentExpression);

                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression(expression as BinaryExpression);

                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression(expression as LiteralExpression);

                case SyntaxKind.NameExpression:
                    return BindNameExpression(expression as NameExpression);

                case SyntaxKind.ParenthesesExpression:
                    return BindParenthesizedExpression(expression as ParenthesizedExpression);

                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression(expression as UnaryExpression);

                default:
                    throw new Exception($"Unexpected syntax node '{expression.Kind}'");
            }
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpression assignment)
        {
            var name = assignment.IdentifierToken.Text;
            var expression = BindExpression(assignment.Expression);

            var defaultValue = 
                expression.Type == typeof(int)
                ? (object)default(int)
                : expression.Type == typeof(bool)
                ? (object)default(bool)
                : throw new Exception($"Unsupported variable type '{expression.Type}'");

            variables[name] = default;

            return new BoundAssignmentExpression(name, expression);
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
            if (!variables.TryGetValue(name, out var value))
            {
                diagnostics.Binding.UndefinedIdentifier(nameExpression);
            }

            var type = value?.GetType() ?? typeof(object);
            return new BoundVariableExpression(name, type);
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