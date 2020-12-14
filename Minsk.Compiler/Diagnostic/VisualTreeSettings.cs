using System;

namespace Minsk.Compiler.Diagnostic
{
    public sealed class VisualTreeSettings
    {
        public int TopPadding = 1;
        public int BottomPadding = 1;
        public int LeftPadding = 2;
        public int TextPadding = 1;
        public int ChildPadding = 1;

        public ConsoleColor NodeBackground = ConsoleColor.White;
        public ConsoleColor NodeForeground = ConsoleColor.Black;
        public ConsoleColor LinkForeground = ConsoleColor.Red;
        public ConsoleColor LinkBackground = ConsoleColor.Black;
    }
}