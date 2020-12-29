using System.Collections.Generic;
using System.Collections.Immutable;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
        {
            Function = function;
            Arguments = arguments;
        }

        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }

        public override TypeSymbol Type => Function.ReturnType;

        public override IEnumerable<BoundNode> Children => Arguments;

        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

        protected override string PrettyPrintText()
            => Function.Name;
    }
}