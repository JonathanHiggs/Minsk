using System.Collections.Immutable;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(
            BoundGlobalScope previous,
            DiagnosticBag diagnostics,
            ImmutableArray<VariableSymbol> variables,
            BoundStatement statement)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variables = variables;
            Statement = statement;
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundStatement Statement { get; }
    }
}