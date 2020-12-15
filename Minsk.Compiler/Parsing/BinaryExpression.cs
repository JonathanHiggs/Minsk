using System;
using System.Collections.Generic;

namespace Minsk.Compiler.Parsing
{
    public sealed class BinaryExpression : Expression
    {
        public BinaryExpression(Expression left, OperatorNode operatorNode, Expression right)
        {
            Left = left 
                ?? throw new ArgumentNullException(nameof(left));

            OperatorNode = operatorNode 
                ?? throw new ArgumentNullException(nameof(operatorNode));

            Right = right 
                ?? throw new ArgumentNullException(nameof(right));

            Left.Parent = this;
            OperatorNode.Parent = this;
            Right.Parent = this;
        }


        public OperatorNode OperatorNode { get; }
        public Expression Left { get; }
        public Expression Right { get; }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public override string Text => OperatorNode.Text;

        public override IEnumerable<SyntaxNode> Children 
        {
            get
            {
                yield return Left;
                yield return OperatorNode;
                yield return Right;
            }
        }
    }
}