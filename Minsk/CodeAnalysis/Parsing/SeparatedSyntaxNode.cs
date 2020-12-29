
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class SeparatedSyntaxNode<T>
        where T : SyntaxNode
    {
        public SeparatedSyntaxNode(T node, LexToken separator)
        {
            Node = node;
            Separator = separator;
        }

        public T Node { get; }
        public LexToken Separator { get; }
    }
}