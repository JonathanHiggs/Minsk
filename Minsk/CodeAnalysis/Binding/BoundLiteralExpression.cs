using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
            Type = TypeSymbol.FromValue(value);
        }

        public override TypeSymbol Type { get; }
        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public object Value { get; }

        public override IEnumerable<BoundNode> Children
            => Enumerable.Empty<BoundNode>();

        protected override string PrettyPrintText()
            => $"{Type.Name}:{Value}";
    }
}