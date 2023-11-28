using System.Linq;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Symbols;

using Op = Minsk.CodeAnalysis.Binding.BoundBinaryOperatorKind;
using Token = Minsk.CodeAnalysis.Lexing.TokenKind;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(
            TokenKind tokenKind,
            BoundBinaryOperatorKind kind,
            TypeSymbol operandType
        )
            : this(tokenKind, kind, operandType, operandType, operandType)
        { }

        private BoundBinaryOperator(
            TokenKind tokenType,
            BoundBinaryOperatorKind kind,
            TypeSymbol operandType,
            TypeSymbol resultType
        )
            : this(tokenType, kind, operandType, operandType, resultType)
        { }

        private BoundBinaryOperator(
            TokenKind tokenKind,
            BoundBinaryOperatorKind kind,
            TypeSymbol leftType,
            TypeSymbol rightType,
            TypeSymbol resultType)
        {
            TokenKind = tokenKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public TokenKind TokenKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol Type { get; }

        public static BoundBinaryOperator Bind(TokenKind tokenKind, TypeSymbol leftType, TypeSymbol rightType)
            => operators.FirstOrDefault(
                op => op.TokenKind == tokenKind && op.LeftType == leftType && op.RightType == rightType);

        private static BoundBinaryOperator From(TokenKind token, BoundBinaryOperatorKind op, TypeSymbol type)
            => new BoundBinaryOperator(token, op, type);

        private static BoundBinaryOperator From(TokenKind token, BoundBinaryOperatorKind op, TypeSymbol operandType, TypeSymbol resultType)
            => new BoundBinaryOperator(token, op, operandType, resultType);

        private static BoundBinaryOperator[] operators = {
            // Logical
            From(Token.AmpersandAmpersand,   Op.LogicalAnd,      TypeSymbol.Bool),
            From(Token.PipePipe,            Op.LogicalOr,       TypeSymbol.Bool),

            From(Token.EqualsEquals,        Op.Equals,          TypeSymbol.Int,     TypeSymbol.Bool),
            From(Token.BangEquals,          Op.NotEquals,       TypeSymbol.Int,     TypeSymbol.Bool),

            From(Token.EqualsEquals,        Op.Equals,          TypeSymbol.Bool),
            From(Token.BangEquals,          Op.NotEquals,       TypeSymbol.Bool),

            From(Token.Less,                Op.Less,            TypeSymbol.Int,     TypeSymbol.Bool),
            From(Token.LessOrEquals,        Op.LessOrEquals,    TypeSymbol.Int,     TypeSymbol.Bool),
            From(Token.Greater,             Op.Greater,         TypeSymbol.Int,     TypeSymbol.Bool),
            From(Token.GreaterOrEquals,     Op.GreaterOrEquals, TypeSymbol.Int,     TypeSymbol.Bool),

            // Numerical
            From(Token.Plus,                Op.Addition,        TypeSymbol.Int),
            From(Token.Minus,               Op.Subtraction,     TypeSymbol.Int),
            From(Token.Star,                Op.Multiplication,  TypeSymbol.Int),
            From(Token.ForwardSlash,        Op.Division,        TypeSymbol.Int),

            // Bitwise
            From(Token.Ampersand,           Op.BitwiseAnd,      TypeSymbol.Int),
            From(Token.Pipe,                Op.BitwiseOr,       TypeSymbol.Int),
            From(Token.Hat,                 Op.BitwiseXor,      TypeSymbol.Int),
            From(Token.Ampersand,           Op.BitwiseAnd,      TypeSymbol.Bool),
            From(Token.Pipe,                Op.BitwiseOr,       TypeSymbol.Bool),
            From(Token.Hat,                 Op.BitwiseXor,      TypeSymbol.Bool),

            // String
            From(Token.Plus,                Op.Addition,        TypeSymbol.String),
            From(Token.EqualsEquals,        Op.Equals,          TypeSymbol.String,  TypeSymbol.Bool),
            From(Token.BangEquals,          Op.NotEquals,       TypeSymbol.String,  TypeSymbol.Bool),
        };
    }
}