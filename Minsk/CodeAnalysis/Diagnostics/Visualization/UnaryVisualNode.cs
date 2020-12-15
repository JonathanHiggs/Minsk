using System;

namespace Minsk.CodeAnalysis.Diagnostics.Visualization
{
    public sealed class UnaryVisualNode : VisualNode
    {
        public UnaryVisualNode(string text, string nodeType, VisualNode child, VisualTreeSettings settings) 
            : base(text, nodeType, settings)
        {
            Child = child ?? throw new ArgumentNullException(nameof(child));
            Child.Parent = this;
        }

        public VisualNode Child { get; }

        public override int CombinedWidth => Math.Max(Width, Child.CombinedWidth);

        public override int CombinedHeight => 2 + Child.CombinedHeight;

        public override void Arange(int left, int top)
        {
            var combinedWidth = CombinedWidth;

            if (Width == combinedWidth)
            {
                LeftPosition = left;
                TopPosition = top;

                var leftPad = (combinedWidth - Child.Width) / 2;
                Child.Arange(left + leftPad, top + 2);
            }
            else
            {
                var leftPad = (combinedWidth - Width) / 2;
                LeftPosition = left + leftPad;
                TopPosition = top;

                Child.Arange(left, top + 2);
            }
        }

        public override void PrintNode()
        {
            PrintText(LeftPosition, TopPosition);
            PrintVerticalLink(LeftPosition + Width / 2, TopPosition + 1);
            Child.PrintNode();
        }
    }
}