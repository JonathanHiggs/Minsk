using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    // ToDo: maybe change to statement in the future
    public sealed class AssignmentExpression : Expression
    {
        public AssignmentExpression(
            LexToken identifierToken, 
            LexToken equalsToken, 
            Expression expression)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;
        }

        public override SyntaxKind Kind 
            => SyntaxKind.AssignmentExpression;

        public override string Text => EqualsToken.Text;
        
        public override string LongText => Text;

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