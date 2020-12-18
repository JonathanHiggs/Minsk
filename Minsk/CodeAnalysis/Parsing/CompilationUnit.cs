using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class CompilationUnit : SyntaxNode
    {
        public CompilationUnit(Statement statement, LexToken endOfFileToken)
        {
            Statement = statement;
            EndOfFileToken = endOfFileToken;
        }

        public Statement Statement { get; }
        public LexToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Statement; } }

        public override LexToken FirstToken => Statement.FirstToken;

        public override LexToken LastToken => EndOfFileToken;
    }
}
