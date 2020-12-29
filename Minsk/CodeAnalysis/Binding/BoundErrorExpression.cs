using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public BoundErrorExpression()
        { }

        public override TypeSymbol Type => TypeSymbol.Error;

        public override IEnumerable<BoundNode> Children => Enumerable.Empty<BoundNode>();

        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;

        protected override string PrettyPrintText() => string.Empty;
    }
}