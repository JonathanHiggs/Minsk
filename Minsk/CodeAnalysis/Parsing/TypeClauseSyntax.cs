using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxTree syntaxTree, LexToken colonToken, LexToken identifier)
            : base(syntaxTree)
        {
            ColonToken = colonToken;
            Identifier = identifier;
        }

        public LexToken ColonToken { get; }
        public LexToken Identifier { get; }

        public override string Text => string.Empty;

        public override LexToken FirstToken => ColonToken;

        public override LexToken LastToken => Identifier;

        public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
    }
}