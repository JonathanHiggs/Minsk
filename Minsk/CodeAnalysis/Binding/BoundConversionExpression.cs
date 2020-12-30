using System.Collections.Generic;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(BoundExpression expression, TypeSymbol toType)
        {
            Expression = expression;
            Type = toType;
        }

        public BoundExpression Expression { get; }

        public override TypeSymbol Type { get; }

        public override IEnumerable<BoundNode> Children { get { yield return Expression; } }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;

        protected override string PrettyPrintText() => Type.Name;
    }
}
