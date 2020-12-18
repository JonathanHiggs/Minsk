using System.Collections.Generic;
using System.Collections.Immutable;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class BlockStatement : Statement
    {
        public BlockStatement(
            LexToken openBraceToken,
            ImmutableArray<Statement> statements,
            LexToken closeBraceToken)
        {
            OpenBraceToken = openBraceToken;
            Statements = statements;
            CloseBraceToken = closeBraceToken;
        }

        public LexToken OpenBraceToken { get; }
        public ImmutableArray<Statement> Statements { get; }
        public LexToken CloseBraceToken { get; }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children => Statements;

        public override LexToken FirstToken => OpenBraceToken;

        public override LexToken LastToken => CloseBraceToken;
    }
}