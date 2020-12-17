using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class NameExpression : Expression
    {
        public NameExpression(LexToken identifierToken)
        {
            IdentifierToken = identifierToken;
        }

        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        public override string Text => IdentifierToken.Text;

        public override IEnumerable<SyntaxNode> Children
            => Enumerable.Empty<SyntaxNode>();

        public override LexToken FirstToken => IdentifierToken;
        public override LexToken LastToken => IdentifierToken;

        public LexToken IdentifierToken { get; }
    }
}