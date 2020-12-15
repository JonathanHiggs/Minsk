using System;
using System.Collections.Generic;
using System.Linq;

namespace Minsk.Compiler.Parsing
{
    public abstract class SyntaxNode
    {
        // ToDo: rename to Kind
        public abstract NodeType NodeType { get; }

        public abstract string Text { get; }

        public SyntaxNode Parent { set; get; }

        public abstract IEnumerable<SyntaxNode> Children { get; }

        public void PrettyPrint(string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.WriteLine($"{NodeType} : {Text}");

            indent += isLast ? "   " : "│  ";

            var lastChild = Children.LastOrDefault();

            foreach (var child in Children)
                child.PrettyPrint(indent, child == lastChild);
        }    
    }
}