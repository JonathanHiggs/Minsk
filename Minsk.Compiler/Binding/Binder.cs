using System;
using System.Collections.Generic;

using Minsk.Compiler.Diagnostic;
using Minsk.Compiler.Parsing;

namespace Minsk.Compiler.Binding
{
    internal sealed class Binder
    {
        private List<CompilerError> errors = new List<CompilerError>();

        public IEnumerable<CompilerError> Errors => errors;

        public BoundExpression BindExpression(Expression expression)
        {
            switch (expression.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression(expression as LiteralExpression);

                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression(expression as UnaryExpression);

                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression(expression as BinaryExpression);

                case SyntaxKind.ParenthesesExpression:
                    return BindExpression((expression as ParenthesizedExpression).Expression);

                default:
                    throw new Exception($"Unexpected syntax node '{expression.Kind}'");
            }
        }

        private BoundLiteralExpression BindLiteralExpression(LiteralExpression literalExpression)
        {
            var value = literalExpression.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpression unaryExpression)
        {
            var operand = BindExpression(unaryExpression.Operand);

            var opToken = unaryExpression.OperatorToken;

            var op = BoundUnaryOperator.Bind(
                opToken.Kind,
                operand.Type);

            if (op is null)
            {
                errors.Add(new BinderError(
                    unaryExpression, 
                    $"Unary operator '{opToken.Kind}' is not defined for type {operand.Type}"));

                return operand;
            }

            return new BoundUnaryExpression(op, operand);
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
                errors.Add(new BinderError(
                    binaryExpression, 
                    $"Binary operator '{opToken.Kind}' is not defined for types {left.Type} and {right.Type}"));

                return left;
            }

            return new BoundBinaryExpression(left, op, right);
        }
    }
}