using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class LiteralExpression : Expression
    {
        public LiteralExpression(LexToken token)
            : this(token, token.Value)
        { }

        public LiteralExpression(LexToken token, object value)
        {
            Token = token 
                ?? throw new ArgumentNullException(nameof(token));

            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

        public LexToken Token { get; }

        public object Value { get; }

        public override string Text => Token.Text;

        public override string LongText => Token.Text;

        public override IEnumerable<SyntaxNode> Children 
            => Enumerable.Empty<SyntaxNode>();

        public override LexToken FirstToken => Token;
        public override LexToken LastToken => Token;
    }
}