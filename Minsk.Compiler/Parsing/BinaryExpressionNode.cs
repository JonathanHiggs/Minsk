using System;
using System.Collections.Generic;

namespace Minsk.Compiler.Parsing
{
    public sealed class BinaryExpressionNode : ExpressionSyntaxNode
    {
        public BinaryExpressionNode(ExpressionSyntaxNode left, OperatorSyntaxNode operatorNode, ExpressionSyntaxNode right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            OperatorNode = operatorNode ?? throw new ArgumentNullException(nameof(operatorNode));
            Right = right ?? throw new ArgumentNullException(nameof(right));

            Left.Parent = this;
            OperatorNode.Parent = this;
            Right.Parent = this;
        }


        public OperatorSyntaxNode OperatorNode { get; }
        public ExpressionSyntaxNode Left { get; }
        public ExpressionSyntaxNode Right { get; }

        public override NodeType NodeType => NodeType.BinaryExpression;

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