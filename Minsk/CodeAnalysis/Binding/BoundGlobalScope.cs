using System;
using System.Collections.Generic;
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
            IEnumerable<VariableSymbol> variables,
            BoundStatement statement
        )
            : this(previous, diagnostics, variables.ToImmutableArray(), statement)
        { }

        public BoundGlobalScope(
            BoundGlobalScope previous,
            DiagnosticBag diagnostics,
            ImmutableArray<VariableSymbol> variables,
            BoundStatement statement)
        {
            Previous = previous;

            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            Variables = variables;

            Statement = statement
                ?? throw new ArgumentNullException(nameof(statement));
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundStatement Statement { get; }
    }
}