using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class UnaryExpression : Expression
    {
        public UnaryExpression(SyntaxTree syntaxTree, LexToken operatorToken, Expression operand)
            : base(syntaxTree)
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

        public override LexToken FirstToken => OperatorToken;
        public override LexToken LastToken => Operand.LastToken;
    }
}