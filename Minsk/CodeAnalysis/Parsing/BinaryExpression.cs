using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class BinaryExpression : Expression
    {
        public BinaryExpression(Expression left, LexToken operatorToken, Expression right)
        {
            Left = left
                ?? throw new ArgumentNullException(nameof(left));

            OperatorToken = operatorToken
                ?? throw new ArgumentNullException(nameof(operatorToken));

            Right = right
                ?? throw new ArgumentNullException(nameof(right));

            Left.Parent = this;
            Right.Parent = this;
        }


        public LexToken OperatorToken { get; }
        public Expression Left { get; }
        public Expression Right { get; }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public override string Text => OperatorToken.Text;

        public override string LongText => $"{Left.LongText} {OperatorToken.Text} {Right.LongText}";

        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }

        public override LexToken FirstToken => Left.FirstToken;
        public override LexToken LastToken => Right.LastToken;
    }
}