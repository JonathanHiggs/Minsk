using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class OperatorNode : SyntaxNode
    {
        public OperatorNode(SyntaxToken operatorToken)
        {
            Token = operatorToken 
                ?? throw new ArgumentNullException(nameof(operatorToken));
        }

        public SyntaxToken Token { get; }

        public override NodeType NodeType => NodeType.Operator;

        public override string Text => Token.Text;

        public override IEnumerable<SyntaxNode> Children 
            => Enumerable.Empty<SyntaxNode>();
    }
}