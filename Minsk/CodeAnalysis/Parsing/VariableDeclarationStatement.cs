using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class VariableDeclarationStatement : Statement
    {
        public VariableDeclarationStatement(
            LexToken keywordToken,
            LexToken identifierToken,
            TypeClauseSyntax optionalTypeClause,
            LexToken equalsToken,
            Expression expression)
        {
            KeywordToken = keywordToken;
            Identifier = identifierToken;
            OptionalTypeClause = optionalTypeClause;
            EqualsToken = equalsToken;
            Expression = expression;
        }

        public LexToken KeywordToken { get; }
        public LexToken Identifier { get; }
        public TypeClauseSyntax OptionalTypeClause { get; }
        public LexToken EqualsToken { get; }
        public Expression Expression { get; }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        { get { yield return Expression; } }

        public override LexToken FirstToken => KeywordToken;

        public override LexToken LastToken => Expression.LastToken;
    }
}