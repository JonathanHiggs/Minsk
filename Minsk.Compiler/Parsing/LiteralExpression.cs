using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
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

        public override IEnumerable<SyntaxNode> Children 
            => Enumerable.Empty<SyntaxNode>();
    }
}