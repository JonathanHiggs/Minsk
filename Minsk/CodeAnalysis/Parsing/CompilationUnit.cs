using System;
using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class CompilationUnit : SyntaxNode
    {
        public CompilationUnit(Expression expression, LexToken endOfFileToken)
        {
            Expression = expression;
            EndOfFileToken = endOfFileToken;
        }

        public Expression Expression { get; }
        public LexToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        public override string Text => "CompilationUnit";

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Expression; } }

        public override LexToken FirstToken => Expression.FirstToken;

        public override LexToken LastToken => EndOfFileToken;
    }
}
