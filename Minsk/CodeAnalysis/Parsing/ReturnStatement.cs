using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ReturnStatement : Statement
    {
        public ReturnStatement(LexToken returnKeyword, Expression expression)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
        }

        public LexToken ReturnKeyword { get; }
        public Expression Expression { get; }

        public override string Text => string.Empty;

        public override LexToken FirstToken => ReturnKeyword;

        public override LexToken LastToken => Expression.LastToken;

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Expression; } }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
    }
}
