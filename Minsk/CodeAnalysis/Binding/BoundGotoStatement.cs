using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement(BoundLabel label)
        {
            Label = label;
        }

        public BoundLabel Label { get; }

        public override IEnumerable<BoundNode> Children => Enumerable.Empty<BoundNode>();

        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;

        protected override string PrettyPrintText()
            => Label.Name;
    }
}