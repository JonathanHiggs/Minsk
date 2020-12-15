using System;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Binding
{
    internal sealed class BoundBinaryOperator 
    {
        private BoundBinaryOperator(
            TokenType tokenKind, 
            BoundBinaryOperatorKind kind, 
            Type operandType
        )
            : this(tokenKind, kind, operandType, operandType, operandType)
        { }
        
        private BoundBinaryOperator(
            TokenType tokenKind, 
            BoundBinaryOperatorKind kind, 
            Type leftType, 
            Type rightType, 
            Type resultType)
        {
            TokenKind = tokenKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        public TokenType TokenKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type ResultType { get; }

        public static BoundBinaryOperator Bind(TokenType tokenKind, Type leftType, Type rightType)
            => operators.FirstOrDefault(
                op => op.TokenKind == tokenKind && op.LeftType == leftType && op.RightType == rightType);

        private static BoundBinaryOperator[] operators = {
            // Logical
            new BoundBinaryOperator(TokenType.AmpersandAmperand, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
            new BoundBinaryOperator(TokenType.PipePipe,          BoundBinaryOperatorKind.LogicalOr,  typeof(bool)),

            // Numerical
            new BoundBinaryOperator(TokenType.Plus,         BoundBinaryOperatorKind.Addition,       typeof(int)),
            new BoundBinaryOperator(TokenType.Minus,        BoundBinaryOperatorKind.Subtraction,    typeof(int)),
            new BoundBinaryOperator(TokenType.Star,         BoundBinaryOperatorKind.Multiplication, typeof(int)),
            new BoundBinaryOperator(TokenType.ForwardSlash, BoundBinaryOperatorKind.Division,       typeof(int)),
        };
    }
}