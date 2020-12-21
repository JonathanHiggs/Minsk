using System;
using System.Collections.Generic;
using System.Linq;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
        }

        public override Type Type => Value.GetType();
        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public object Value { get; }

        public override IEnumerable<BoundNode> Children
            => Enumerable.Empty<BoundNode>();

        protected override string PrettyPrintText()
            => $"{Value}, {Type.Name}";
    }
}