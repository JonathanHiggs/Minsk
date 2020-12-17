using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract string Text { get; }

        public SyntaxNode Parent { set; get; }

        public abstract IEnumerable<SyntaxNode> Children { get; }

        public abstract LexToken FirstToken { get; }
        public abstract LexToken LastToken { get; }

        public void PrettyPrint(string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.WriteLine($"{Kind}  {Text}");

            indent += isLast ? "   " : "│  ";

            var lastChild = Children.LastOrDefault();

            foreach (var child in Children)
                child.PrettyPrint(indent, child == lastChild);
        }
    }
}