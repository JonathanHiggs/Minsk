using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    // ToDo: maybe remove
    public sealed class OperatorNode : SyntaxNode
    {
        public OperatorNode(LexToken token)
        {
            Token = token 
                ?? throw new ArgumentNullException(nameof(token));
        }

        public LexToken Token { get; }

        public override SyntaxKind Kind => SyntaxKind.OperatorNode;

        public override string Text => Token.Text;

        public override IEnumerable<SyntaxNode> Children 
            => Enumerable.Empty<SyntaxNode>();
    }
}