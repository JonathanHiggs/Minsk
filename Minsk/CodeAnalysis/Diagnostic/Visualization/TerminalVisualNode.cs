namespace Minsk.CodeAnalysis.Diagnostic.Visualization
{
    public sealed class TerminalVisualNode : VisualNode
    {
        public TerminalVisualNode(string text, string nodeType, VisualTreeSettings settings) 
            : base(text, nodeType, settings)
        {  }

        public override int CombinedWidth => Width;

        public override int CombinedHeight => 1;

        public override void Arange(int left, int top)
        {
            LeftPosition = left;
            TopPosition = top;
        }

        public override void PrintNode()
        {
            PrintText(LeftPosition, TopPosition);
        }
    }
}