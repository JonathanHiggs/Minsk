using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement(LabelSymbol label)
        {
            Label = label;
        }

        public override IEnumerable<BoundNode> Children => Enumerable.Empty<BoundNode>();

        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

        public LabelSymbol Label { get; }

        protected override string PrettyPrintText()
            => $"{Label.Name}:";
    }
}