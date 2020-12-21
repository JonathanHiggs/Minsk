using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Minsk.CodeAnalysis.Common
{
    public abstract class Node<T, TKind> where T : Node<T, TKind>
    {
        public T Parent { get; }

        public abstract IEnumerable<T> Children { get; }

        public abstract TKind Kind { get; }

        protected abstract string PrettyPrintText();

        protected abstract ConsoleColor PrettyPrintColorForKind(TKind kind);

        public void PrettyPrint(TextWriter textWriter, string indent = "", bool isLast = true)
        {
            var toConsole = textWriter == Console.Out;

            var marker = isLast ? "└──" : "├──";

            if (toConsole)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                textWriter.Write(indent);
                textWriter.Write(marker);
                Console.ResetColor();
            }
            else
            {
                textWriter.Write(indent);
                textWriter.Write(marker);
            }


            if (toConsole)
            {
                Console.ForegroundColor = PrettyPrintColorForKind(Kind);
                textWriter.WriteLine($"{Kind}  {PrettyPrintText()}");
                Console.ResetColor();
            }
            else
            {
                textWriter.WriteLine($"{Kind}  {PrettyPrintText()}");
            }

            indent += isLast ? "   " : "│  ";

            var lastChild = Children.LastOrDefault();

            foreach (var child in Children)
                child.PrettyPrint(textWriter, indent, child == lastChild);
        }
    }
}