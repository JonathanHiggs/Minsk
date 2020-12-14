using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class OperatorSyntaxNode : SyntaxNode
    {
        public OperatorSyntaxNode(SyntaxToken token)
        {
            Token = token;
        }

        public SyntaxToken Token { get; }

        public override NodeType NodeType => NodeType.OperatorNode;

        public override string Text => Token.Text;

        public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();
    }
}