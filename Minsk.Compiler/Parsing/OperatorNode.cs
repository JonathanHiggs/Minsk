using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class OperatorNode : SyntaxNode
    {
        public OperatorNode(SyntaxToken token)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public SyntaxToken Token { get; }

        public override NodeType NodeType => NodeType.OperatorNode;

        public override string Text => Token.Text;

        public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();
    }
}