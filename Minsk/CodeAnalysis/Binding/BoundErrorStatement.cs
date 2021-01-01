using System.Collections.Generic;
using System.Linq;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundErrorStatement : BoundStatement
    {
        public override IEnumerable<BoundNode> Children => Enumerable.Empty<BoundNode>();

        public override BoundNodeKind Kind => BoundNodeKind.ErrorStatement;

        protected override string PrettyPrintText() => "Error";
    }
}