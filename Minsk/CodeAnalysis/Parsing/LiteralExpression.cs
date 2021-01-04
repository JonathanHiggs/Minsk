using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class LiteralExpression : Expression
    {
        public LiteralExpression(SyntaxTree syntaxTree, LexToken token)
            : this(syntaxTree, token, token.Value)
        { }

        public LiteralExpression(SyntaxTree syntaxTree, LexToken token, object value)
            : base(syntaxTree)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));

            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

        public LexToken Token { get; }

        public object Value { get; }

        public override string Text => Token.Text;

        public override IEnumerable<SyntaxNode> Children
            => Enumerable.Empty<SyntaxNode>();

        public override LexToken FirstToken => Token;
        public override LexToken LastToken => Token;
    }
}