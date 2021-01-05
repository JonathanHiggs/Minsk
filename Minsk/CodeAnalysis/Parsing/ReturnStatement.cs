using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ReturnStatement : Statement
    {
        public ReturnStatement(SyntaxTree syntaxTree, LexToken returnKeyword, Expression optionalExpression)
            : base(syntaxTree)
        {
            ReturnKeyword = returnKeyword;
            OptionalExpression = optionalExpression;

            if (OptionalExpression is not null)
                OptionalExpression.Parent = this;
        }

        public LexToken ReturnKeyword { get; }
        public Expression OptionalExpression { get; }

        public override string Text => string.Empty;

        public override LexToken FirstToken => ReturnKeyword;

        public override LexToken LastToken => OptionalExpression.LastToken;

        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                if (OptionalExpression is not null)
                    yield return OptionalExpression;
            }
        }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
    }
}
