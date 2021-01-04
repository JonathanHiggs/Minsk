using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class VariableDeclarationStatement : Statement
    {
        public VariableDeclarationStatement(
            SyntaxTree syntaxTree,
            LexToken keywordToken,
            LexToken identifierToken,
            TypeClauseSyntax optionalTypeClause,
            LexToken equalsToken,
            Expression initializer
        )
            : base(syntaxTree)
        {
            KeywordToken = keywordToken;
            Identifier = identifierToken;
            OptionalTypeClause = optionalTypeClause;
            EqualsToken = equalsToken;
            Initializer = initializer;

            if (OptionalTypeClause is not null)
                OptionalTypeClause.Parent = this;

            Initializer.Parent = this;
        }

        public LexToken KeywordToken { get; }
        public LexToken Identifier { get; }
        public TypeClauseSyntax OptionalTypeClause { get; }
        public LexToken EqualsToken { get; }
        public Expression Initializer { get; }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

        public override string Text => string.Empty;

        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                if (OptionalTypeClause is not null)
                    yield return OptionalTypeClause;
                yield return Initializer;
            }
        }

        public override LexToken FirstToken => KeywordToken;

        public override LexToken LastToken => Initializer.LastToken;
    }
}