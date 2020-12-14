using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public sealed class NumberSyntaxNode : ExpressionSyntaxNode
    {
        public NumberSyntaxNode(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }

        public override NodeType NodeType => NodeType.NumberExpression;

        public SyntaxToken NumberToken { get; }

        public override string Text => NumberToken.Text;

        public override IEnumerable<SyntaxNode> Children => Enumerable.Empty<SyntaxNode>();

    }
}