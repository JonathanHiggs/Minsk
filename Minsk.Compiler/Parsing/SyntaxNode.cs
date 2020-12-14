using System.Collections.Generic;

namespace Minsk.Compiler.Parsing
{
    public abstract class SyntaxNode
    {
        public abstract NodeType NodeType { get; }

        public abstract string Text { get; }

        public SyntaxNode Parent { set; get; }

        public abstract IEnumerable<SyntaxNode> Children { get; }
    }
}