using System;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(
            TokenKind tokenKind,
            BoundBinaryOperatorKind kind,
            Type operandType
        )
            : this(tokenKind, kind, operandType, operandType, operandType)
        { }

        private BoundBinaryOperator(
            TokenKind tokenType,
            BoundBinaryOperatorKind kind,
            Type operandType,
            Type resultType
        )
            : this(tokenType, kind, operandType, operandType, resultType)
        { }

        private BoundBinaryOperator(
            TokenKind tokenKind,
            BoundBinaryOperatorKind kind,
            Type leftType,
            Type rightType,
            Type resultType)
        {
            TokenKind = tokenKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public TokenKind TokenKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type Type { get; }

        public static BoundBinaryOperator Bind(TokenKind tokenKind, Type leftType, Type rightType)
            => operators.FirstOrDefault(
                op => op.TokenKind == tokenKind && op.LeftType == leftType && op.RightType == rightType);

        private static BoundBinaryOperator[] operators = {
            // Logical
            new BoundBinaryOperator(TokenKind.AmpersandAmperand, BoundBinaryOperatorKind.LogicalAnd,        typeof(bool)),
            new BoundBinaryOperator(TokenKind.PipePipe,          BoundBinaryOperatorKind.LogicalOr,         typeof(bool)),

            new BoundBinaryOperator(TokenKind.EqualsEquals,      BoundBinaryOperatorKind.Equals,            typeof(int),  typeof(bool)),
            new BoundBinaryOperator(TokenKind.BangEquals,        BoundBinaryOperatorKind.NotEquals,         typeof(int),  typeof(bool)),

            new BoundBinaryOperator(TokenKind.EqualsEquals,      BoundBinaryOperatorKind.Equals,            typeof(bool)),
            new BoundBinaryOperator(TokenKind.BangEquals,        BoundBinaryOperatorKind.NotEquals,         typeof(bool)),

            new BoundBinaryOperator(TokenKind.Less,              BoundBinaryOperatorKind.Less,              typeof(int),  typeof(bool)),
            new BoundBinaryOperator(TokenKind.LessOrEquals,      BoundBinaryOperatorKind.LessOrEquals,      typeof(int),  typeof(bool)),
            new BoundBinaryOperator(TokenKind.Greater,           BoundBinaryOperatorKind.Greater,           typeof(int),  typeof(bool)),
            new BoundBinaryOperator(TokenKind.GreaterOrEquals,   BoundBinaryOperatorKind.GreaterOrEquals,   typeof(int),  typeof(bool)),

            // Numerical
            new BoundBinaryOperator(TokenKind.Plus,              BoundBinaryOperatorKind.Addition,          typeof(int)),
            new BoundBinaryOperator(TokenKind.Minus,             BoundBinaryOperatorKind.Subtraction,       typeof(int)),
            new BoundBinaryOperator(TokenKind.Star,              BoundBinaryOperatorKind.Multiplication,    typeof(int)),
            new BoundBinaryOperator(TokenKind.ForwardSlash,      BoundBinaryOperatorKind.Division,          typeof(int)),

            // Bitwise
            new BoundBinaryOperator(TokenKind.Ampersand,        BoundBinaryOperatorKind.BitwiseAnd,         typeof(int)),
            new BoundBinaryOperator(TokenKind.Pipe,             BoundBinaryOperatorKind.BitwiseOr,          typeof(int)),
            new BoundBinaryOperator(TokenKind.Hat,           BoundBinaryOperatorKind.BitwiseXor,         typeof(int)),
            new BoundBinaryOperator(TokenKind.Ampersand,        BoundBinaryOperatorKind.BitwiseAnd,         typeof(bool)),
            new BoundBinaryOperator(TokenKind.Pipe,             BoundBinaryOperatorKind.BitwiseOr,          typeof(bool)),
            new BoundBinaryOperator(TokenKind.Hat,           BoundBinaryOperatorKind.BitwiseXor,         typeof(bool)),
        };
    }
}