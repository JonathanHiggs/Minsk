using System;
using System.Collections.Generic;

using Minsk.Compiler.Core;
using Minsk.Compiler.Lexing;
using Minsk.Compiler.Parsing;

namespace Minsk.Compiler.Binding
{
    internal sealed class Binder
    {
        private List<CompilerError> errors = new List<CompilerError>();

        public IEnumerable<CompilerError> Errors => errors;

        public BoundExpression BindExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case NodeType.NumberLiteral:
                    return BindLiteralExpression(expression as LiteralExpression);

                case NodeType.UnaryExpression:
                    return BindUnaryExpression(expression as UnaryExpression);

                case NodeType.BinaryExpression:
                    return BindBinaryExpression(expression as BinaryExpression);

                default:
                    throw new Exception($"Unexpected syntax node '{expression.NodeType}'");
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

            var operatorKind = BindUnaryOperatorKind(
                unaryExpression.OperatorNode.Token.TokenType,
                operand.Type);

            if (operatorKind is null)
            {
                errors.Add(new BinderError(
                    unaryExpression, 
                    $"Unary operator '{unaryExpression.OperatorNode.Token.Text}' is not defined for type {operand.Type}"));

                return operand;
            }

            return new BoundUnaryExpression(operatorKind.Value, operand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpression binaryExpression)
        {
            var left = BindExpression(binaryExpression.Left);
            var right = BindExpression(binaryExpression.Right);

            var operatorKind = BindBinaryOperatorKind(
                binaryExpression.OperatorNode.Token.TokenType,
                left.Type,
                right.Type);

            if (operatorKind is null)
            {
                errors.Add(new BinderError(
                    binaryExpression, 
                    $"Binary operator '{binaryExpression.OperatorNode.Token.Text}' is not defined for types {left.Type} and {right.Type}"));

                return left;
            }

            return new BoundBinaryExpression(left, operatorKind.Value, right);
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(TokenType tokenType, Type operandType)
        {
            if (operandType == typeof(int))
            {
                switch (tokenType)
                {
                    case TokenType.Plus:
                        return BoundUnaryOperatorKind.Identity;

                    case TokenType.Minus:
                        return BoundUnaryOperatorKind.Negation;
                };
            }
            else if (operandType == typeof(bool))
            {
                switch(tokenType)
                {
                    case TokenType.Bang:
                        return BoundUnaryOperatorKind.LogicalNegation;
                }
            }

            return null;
        }

        private BoundBinaryOperatorKind? BindBinaryOperatorKind(TokenType tokenType, Type leftType, Type rightType)
        {
            if (leftType == typeof(int) && rightType == typeof(int))
            {
                switch (tokenType)
                {
                    case TokenType.Plus:
                        return BoundBinaryOperatorKind.Addition;

                    case TokenType.Minus:
                        return BoundBinaryOperatorKind.Subtraction;

                    case TokenType.Star:
                        return BoundBinaryOperatorKind.Multiplication;

                    case TokenType.ForwardSlash:
                        return BoundBinaryOperatorKind.Division;
                };
            }
            else if (leftType == typeof(bool) && rightType == typeof(bool))
            { 
                switch (tokenType)
                {
                    case TokenType.AmpersandAmperand:
                        return BoundBinaryOperatorKind.LogicalAnd;

                    case TokenType.PipePipe:
                        return BoundBinaryOperatorKind.LogicalOr;
                }
            }

            return null;
        }
    }
}