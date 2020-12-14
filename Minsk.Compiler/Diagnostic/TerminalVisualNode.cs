namespace Minsk.Compiler.Diagnostic
{
    public sealed class TerminalVisualNode : VisualNode
    {
        public TerminalVisualNode(string text, VisualTreeSettings settings) 
            : base(text, settings)
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