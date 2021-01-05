using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class AssignmentExpression : Expression
    {
        public AssignmentExpression(
            SyntaxTree syntaxTree,
            LexToken identifier,
            LexToken equalsToken,
            Expression expression
        )
            : base(syntaxTree)
        {
            Identifier = identifier;
            EqualsToken = equalsToken;
            Expression = expression;

            Expression.Parent = this;
        }

        public override SyntaxKind Kind
            => SyntaxKind.AssignmentExpression;

        public override string Text => $"{Identifier.Text} {EqualsToken.Text}";

        public LexToken Identifier { get; }
        public LexToken EqualsToken { get; }
        public Expression Expression { get; }

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Expression; } }

        public override LexToken FirstToken
            => Identifier;

        public override LexToken LastToken
            => Expression.LastToken;

    }
}