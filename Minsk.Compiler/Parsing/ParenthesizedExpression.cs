using System;
using System.Collections.Generic;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class ParenthesizedExpression : Expression
    {
        public ParenthesizedExpression(
            SyntaxToken openParentheses,
            Expression expression,
            SyntaxToken closeParenteses)
        {
            OpenParentheses = openParentheses
                ?? throw new ArgumentNullException(nameof(openParentheses));

            Expression = expression
                ?? throw new ArgumentNullException(nameof(expression));

            CloseParentheses = closeParenteses
                ?? throw new ArgumentNullException(nameof(closeParenteses));

            Expression.Parent = this;
        }

        public SyntaxToken OpenParentheses { get; }
        public Expression Expression { get; }
        public SyntaxToken CloseParentheses { get; }

        public override NodeType NodeType => NodeType.ParenthesesExpression;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        {
            get { yield return Expression; }
        }
    }
}