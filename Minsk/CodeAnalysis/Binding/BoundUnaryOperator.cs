using System.Linq;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Symbols;

using Op = Minsk.CodeAnalysis.Binding.BoundUnaryOperatorKind;
using Token = Minsk.CodeAnalysis.Lexing.TokenKind;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(
            TokenKind tokenKind,
            BoundUnaryOperatorKind kind,
            TypeSymbol operandType
        )
            : this(tokenKind, kind, operandType, operandType)
        { }

        private BoundUnaryOperator(
            TokenKind tokenKind,
            BoundUnaryOperatorKind kind,
            TypeSymbol operandType,
            TypeSymbol resultType)
        {
            TokenKind = tokenKind;
            Kind = kind;
            OperandType = operandType;
            Type = resultType;
        }

        public TokenKind TokenKind { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol Type { get; }

        public static BoundUnaryOperator Bind(TokenKind tokenKind, TypeSymbol operandType)
            => operators.FirstOrDefault(
                op => op.TokenKind == tokenKind && op.OperandType == operandType);

        private static BoundUnaryOperator From(TokenKind tokenKind, BoundUnaryOperatorKind op, TypeSymbol type)
            => new BoundUnaryOperator(tokenKind, op, type);

        private static BoundUnaryOperator[] operators = {
            // Logical
            From(Token.Bang,    Op.LogicalNegation, TypeSymbol.Bool),

            // Numerical
            From(Token.Plus,    Op.Identity,        TypeSymbol.Int),
            From(Token.Minus,   Op.Negation,        TypeSymbol.Int),

            // Bitwise
            From(Token.Tilde,   Op.OnesCompliment,  TypeSymbol.Int),
        };
    }
}