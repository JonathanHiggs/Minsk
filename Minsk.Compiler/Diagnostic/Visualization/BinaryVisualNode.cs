using System;

namespace Minsk.Compiler.Diagnostic.Visualization
{
    public sealed class BinaryVisualNode : VisualNode
    {
        public BinaryVisualNode(
            string text,
            string nodeType,
            VisualNode leftChild, 
            VisualNode rightChild, 
            VisualTreeSettings settings
        ) 
            : base(text, nodeType, settings)
        {
            LeftChild = leftChild ?? throw new ArgumentNullException(nameof(leftChild));
            RightChild = rightChild ?? throw new ArgumentNullException(nameof(rightChild));

            LeftChild.Parent = this;
            RightChild.Parent = this;
        }

        public VisualNode LeftChild { get; }
        public VisualNode RightChild { get; }

        public override int CombinedWidth
            => Math.Max(Width, LeftChild.CombinedWidth + Settings.ChildPadding + RightChild.CombinedWidth);

        public override int CombinedHeight
            => 2 + Math.Max(LeftChild.CombinedHeight, RightChild.CombinedHeight);

        public override void Arange(int left, int top)
        {
            var combinedWidth = CombinedWidth;

            if (Width == combinedWidth)
            {
                // Node is wider than child sub-trees
                LeftPosition = left;
                TopPosition = top;

                var leftCombinedWidth = LeftChild.CombinedWidth;
                var rightCombinedWidth = RightChild.CombinedWidth;
                var leftPad = (combinedWidth - leftCombinedWidth - Settings.ChildPadding - rightCombinedWidth) / 2;

                LeftChild.Arange(left + leftPad, top + 2);
                RightChild.Arange(left + leftPad + leftCombinedWidth + Settings.ChildPadding, top + 2);
            }
            else
            {
                LeftChild.Arange(left, top + 2);
                RightChild.Arange(left + LeftChild.CombinedWidth + Settings.ChildPadding, top + 2);

                var childrenWidth = (RightChild.RightPosition - LeftChild.LeftPosition);
                LeftPosition = LeftChild.LeftPosition + childrenWidth / 2 - Width / 2;
                TopPosition = top;
            }
        }

        public override void PrintNode()
        {
            PrintText(LeftPosition, TopPosition);

            PrintLeftLink(LeftChild.LeftPosition + LeftChild.Width / 2, LeftPosition, TopPosition + 1);
            LeftChild.PrintNode();

            PrintRightLink(LeftPosition + Width - 1, RightChild.LeftPosition + RightChild.Width / 2, TopPosition + 1);
            RightChild.PrintNode();
        }
    }
}