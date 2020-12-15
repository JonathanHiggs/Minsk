using System;
using System.Collections.Generic;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class UnaryExpression : Expression
    {
        public UnaryExpression(LexToken operatorToken, Expression operand)
        {
            OperatorToken = operatorToken
                ?? throw new ArgumentNullException(nameof(operatorToken));

            Operand = operand
                ?? throw new ArgumentNullException(nameof(operand));

            Operand.Parent = this;
        }


        public LexToken OperatorToken { get; }
        public Expression Operand { get; }

        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

        public override string Text => OperatorToken.Text;

        public override IEnumerable<SyntaxNode> Children 
        {
            get { yield return Operand; }
        }
    }
}