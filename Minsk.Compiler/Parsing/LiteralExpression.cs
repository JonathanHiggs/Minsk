using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class LiteralExpression : Expression
    {
        public LiteralExpression(SyntaxToken literalToken)
            : this(literalToken, literalToken.Value)
        { }

        public LiteralExpression(SyntaxToken literalToken, object value)
        {
            NumberToken = literalToken 
                ?? throw new ArgumentNullException(nameof(literalToken));

            Value = value;
        }

        public override NodeType NodeType => NodeType.NumberLiteral;

        public SyntaxToken NumberToken { get; }

        public object Value { get; }

        public override string Text => NumberToken.Text;

        public override IEnumerable<SyntaxNode> Children 
            => Enumerable.Empty<SyntaxNode>();
    }
}