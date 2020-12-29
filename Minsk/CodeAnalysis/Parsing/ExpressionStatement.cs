using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ExpressionStatement : Statement
    {
        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Expression; } }

        public override LexToken FirstToken => Expression.FirstToken;

        public override LexToken LastToken => Expression.LastToken;
    }
}