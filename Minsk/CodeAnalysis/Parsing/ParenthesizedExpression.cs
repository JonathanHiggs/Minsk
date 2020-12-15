using System;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ParenthesizedExpression : Expression
    {
        public ParenthesizedExpression(
            LexToken openParentheses,
            Expression expression,
            LexToken closeParenteses)
        {
            OpenParentheses = openParentheses
                ?? throw new ArgumentNullException(nameof(openParentheses));

            Expression = expression
                ?? throw new ArgumentNullException(nameof(expression));

            CloseParentheses = closeParenteses
                ?? throw new ArgumentNullException(nameof(closeParenteses));

            Expression.Parent = this;
        }

        public LexToken OpenParentheses { get; }
        public Expression Expression { get; }
        public LexToken CloseParentheses { get; }

        public override SyntaxKind Kind => SyntaxKind.ParenthesesExpression;

        public override string Text => string.Empty;

        public override string LongText => $"{OpenParentheses.Text}{Expression.LongText}{CloseParentheses.Text}";

        public override IEnumerable<SyntaxNode> Children
        {
            get { yield return Expression; }
        }

        public override LexToken FirstToken => OpenParentheses;
        public override LexToken LastToken => CloseParentheses;
    }
}