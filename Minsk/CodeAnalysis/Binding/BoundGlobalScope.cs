using System.Collections.Immutable;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Diagnostics;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(
            BoundGlobalScope previous,
            DiagnosticBag diagnostics,
            ImmutableArray<VariableSymbol> variables,
            BoundExpression expression)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variables = variables;
            Expression = expression;
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundExpression Expression { get; }
    }
}