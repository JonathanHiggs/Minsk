using System.Collections.Generic;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class ForToStatement : Statement
    {
        public ForToStatement(
            LexToken forKeyword,
            LexToken identifier,
            LexToken equals,
            Expression lowerBound,
            LexToken toKeyword,
            Expression upperBound,
            Statement body)
        {
            ForKeyword = forKeyword;
            Identifier = identifier;
            EqualsToken = equals;
            LowerBound = lowerBound;
            ToKeyword = toKeyword;
            UpperBound = upperBound;
            Body = body;
        }

        public LexToken ForKeyword { get; }
        public LexToken Identifier { get; }
        public LexToken EqualsToken { get; }
        public Expression LowerBound { get; }
        public LexToken ToKeyword { get; }
        public Expression UpperBound { get; }
        public Statement Body { get; }

        public override SyntaxKind Kind => SyntaxKind.ForToStatement;

        public override string Text => $"{Identifier.Text}";

        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                yield return LowerBound;
                yield return UpperBound;
                yield return Body;
            }
        }

        public override LexToken FirstToken => ForKeyword;

        public override LexToken LastToken => Body.LastToken;
    }
}