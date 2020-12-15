using System;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(
            TokenType tokenKind, 
            BoundUnaryOperatorKind kind, 
            Type operandType
        )
            : this(tokenKind, kind, operandType, operandType)
        { }
        
        private BoundUnaryOperator(
            TokenType tokenKind, 
            BoundUnaryOperatorKind kind, 
            Type operandType, 
            Type resultType)
        {
            TokenKind = tokenKind;
            Kind = kind;
            OperandType = operandType;
            Type = resultType;
        }

        public TokenType TokenKind { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public Type OperandType { get; }
        public Type Type { get; }

        public static BoundUnaryOperator Bind(TokenType tokenKind, Type operandType)
            => operators.FirstOrDefault(
                op => op.TokenKind == tokenKind && op.OperandType == operandType);

        private static BoundUnaryOperator[] operators = {
            // Logical
            new BoundUnaryOperator(TokenType.Bang,  BoundUnaryOperatorKind.LogicalNegation, typeof(bool)),

            // Numerical
            new BoundUnaryOperator(TokenType.Plus,  BoundUnaryOperatorKind.Identity,        typeof(int)),
            new BoundUnaryOperator(TokenType.Minus, BoundUnaryOperatorKind.Negation,        typeof(int)),
        };
    }
}