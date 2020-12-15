using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class NumberLiteral : Expression
    {
        public NumberLiteral(SyntaxToken literalToken)
        {
            NumberToken = literalToken 
                ?? throw new ArgumentNullException(nameof(literalToken));
        }

        public override NodeType NodeType => NodeType.NumberLiteral;

        public SyntaxToken NumberToken { get; }

        public override string Text => NumberToken.Text;

        public override IEnumerable<SyntaxNode> Children 
            => Enumerable.Empty<SyntaxNode>();
    }
}