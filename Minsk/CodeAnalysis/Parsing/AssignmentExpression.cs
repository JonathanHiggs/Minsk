using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class AssignmentExpression : Expression
    {
        public AssignmentExpression(
            SyntaxTree syntaxTree,
            LexToken identifierToken,
            LexToken equalsToken,
            Expression expression
        )
            : base(syntaxTree)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;

            Expression.Parent = this;
        }

        public override SyntaxKind Kind
            => SyntaxKind.AssignmentExpression;

        public override string Text => $"{IdentifierToken.Text} {EqualsToken.Text}";

        public LexToken IdentifierToken { get; }
        public LexToken EqualsToken { get; }
        public Expression Expression { get; }

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Expression; } }

        public override LexToken FirstToken
            => IdentifierToken;

        public override LexToken LastToken
            => Expression.LastToken;

    }
}