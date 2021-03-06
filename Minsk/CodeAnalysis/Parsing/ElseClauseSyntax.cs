using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax(SyntaxTree syntaxTree, LexToken elseKeyword, Statement statement)
            : base(syntaxTree)
        {
            ElseKeyword = elseKeyword;
            Statement = statement;

            Statement.Parent = this;
        }

        public LexToken ElseKeyword { get; }
        public Statement Statement { get; }

        public override SyntaxKind Kind => SyntaxKind.ElseNode;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Statement; } }

        public override LexToken FirstToken => ElseKeyword;

        public override LexToken LastToken => Statement.LastToken;
    }
}