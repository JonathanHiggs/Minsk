using System;

namespace Minsk.Compiler.Diagnostic.Visualization
{
    public sealed class VisualTreeSettings
    {
        public int TopPadding = 1;
        public int BottomPadding = 1;
        public int LeftPadding = 2;
        public int TextPadding = 1;
        public int ChildPadding = 1;

        public ConsoleColor NodeBackground = ConsoleColor.DarkGreen;
        public ConsoleColor NodeForeground = ConsoleColor.White;
        public ConsoleColor LinkForeground = ConsoleColor.Green;
        public ConsoleColor LinkBackground = ConsoleColor.Black;

        public bool VerboseNodes = true;
    }
}