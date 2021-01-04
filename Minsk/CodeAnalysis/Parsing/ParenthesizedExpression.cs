using System;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ParenthesizedExpression : Expression
    {
        public ParenthesizedExpression(
            SyntaxTree syntaxTree,
            LexToken openParentheses,
            Expression expression,
            LexToken closeParenteses
        )
            : base(syntaxTree)
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

        public override IEnumerable<SyntaxNode> Children
        {
            get { yield return Expression; }
        }

        public override LexToken FirstToken => OpenParentheses;
        public override LexToken LastToken => CloseParentheses;
    }
}