using System;

namespace Minsk.CodeAnalysis.Diagnostics.Visualization
{
    public abstract class VisualNode
    {
        protected VisualNode(string text, string nodeType, VisualTreeSettings settings)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            NodeType = nodeType ?? throw new ArgumentNullException(nameof(nodeType));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string Text { get; }
        public string NodeType { get; }
        public string DisplayText => Settings.VerboseNodes ? $"{NodeType}: {Text}" : Text;
        public int TopPosition { get; set; }
        public int LeftPosition { get; set; }
        public VisualNode Parent { get; set; }
        public VisualTreeSettings Settings { get; }

        public int Width => DisplayText.Length + Settings.TextPadding * 2;

        public int RightPosition
        {
            get => LeftPosition + Width;
            set => LeftPosition = value - Width;
        }

        public abstract int CombinedWidth { get; }
        public abstract int CombinedHeight { get; }

        public abstract void Arange(int left, int top);

        public void Print()
        {
            // ToDo: adapt to work with TextWriter
            var foreground = Console.ForegroundColor;
            var background = Console.BackgroundColor;

            var top = Console.CursorTop + Settings.TopPadding;

            Arange(Settings.LeftPadding, top);
            PrintNode();

            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.SetCursorPosition(0, top + CombinedHeight + Settings.BottomPadding);
        }

        public abstract void PrintNode();

        protected void PrintText(int left, int top)
        {
            Console.ForegroundColor = Settings.NodeForeground;
            Console.BackgroundColor = Settings.NodeBackground;
            Console.SetCursorPosition(left, top);
            for (var i = 0; i < Settings.TextPadding; i++)
                Console.Write(" ");

            Console.Write(DisplayText);

            for (var i = 0; i < Settings.TextPadding; i++)
                Console.Write(" ");
        }

        protected void PrintVerticalLink(int left, int top)
        {
            Console.ForegroundColor = Settings.LinkForeground;
            Console.BackgroundColor = Settings.LinkBackground;
            Console.SetCursorPosition(left, top);
            Console.Write("|");
        }

        protected void PrintLeftLink(int left, int right, int top)
        {
            if (left == right)
                PrintVerticalLink(left, top);
            else if (left > right)
                PrintVerticalLink(left, top);
            else
                PrintLink("┌", "─", "┘", left, right, top);
        }

        protected void PrintRightLink(int left, int right, int top)
        {
            if (left == right)
                PrintVerticalLink(left, top);
            else if (left > right)
                PrintVerticalLink(right, top);
            else
                PrintLink("└", "─", "┐", left, right, top);
        }

        protected void PrintLink(string left, string space, string right, int leftPos, int rightPos, int top)
        {
            Console.ForegroundColor = Settings.LinkForeground;
            Console.BackgroundColor = Settings.LinkBackground;
            Console.SetCursorPosition(leftPos, top);
            Console.Write(left);
            while (Console.CursorLeft < rightPos)
                Console.Write(space);
            Console.Write(right);
        }
    }
}