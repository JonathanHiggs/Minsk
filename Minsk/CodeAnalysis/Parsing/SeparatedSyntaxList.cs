using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class SeparatedSyntaxList<T> : IEnumerable<T>
        where T : SyntaxNode
    {
        public SeparatedSyntaxList(ImmutableArray<SeparatedSyntaxNode<T>> nodes)
        {
            Nodes = nodes;
        }

        public ImmutableArray<SeparatedSyntaxNode<T>> Nodes { get; }

        public int Count => Nodes.Length;

        public T this[int index] => Nodes[index].Node;

        public LexToken GetSeparator(int index) => Nodes[index].Separator;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}