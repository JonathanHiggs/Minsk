using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variable)
        {
            Variable = variable;
        }

        public VariableSymbol Variable { get; }
        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override TypeSymbol Type => Variable?.Type ?? TypeSymbol.Unknown;

        public override IEnumerable<BoundNode> Children
            => Enumerable.Empty<BoundNode>();

        protected override string PrettyPrintText()
            => $"{Type?.Name ?? "Unknown"}:{Variable?.Name ?? "''"}";

        public override string ToString()
            => $"{Type?.Name ?? "Unknown"}:{Variable?.Name ?? "''"}";
    }
}