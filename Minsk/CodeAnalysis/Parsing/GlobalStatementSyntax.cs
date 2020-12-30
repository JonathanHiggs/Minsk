using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class GlobalStatementSyntax : MemberSyntax
    {
        public GlobalStatementSyntax(Statement statement)
        {
            Statement = statement;
        }

        public Statement Statement { get; }

        public override string Text => string.Empty;

        public override LexToken FirstToken => Statement.FirstToken;

        public override LexToken LastToken => Statement.LastToken;

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Statement; } }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    }
}
