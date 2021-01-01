using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ContinueStatement : Statement
    {
        public ContinueStatement(LexToken keyword)
        {
            Keyword = keyword;
        }

        public LexToken Keyword { get; }

        public override string Text => Keyword.Text;

        public override LexToken FirstToken => Keyword;

        public override LexToken LastToken => Keyword;

        public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
    }
}