using System;
using System.Collections.Generic;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
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

        public override IEnumerable<SyntaxNode> Children 
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }
    }
}