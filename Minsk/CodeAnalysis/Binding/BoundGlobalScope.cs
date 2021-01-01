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
            IEnumerable<FunctionSymbol> functions,
            IEnumerable<VariableSymbol> variables,
            IEnumerable<BoundStatement> statements
        )
            : this(
                  previous,
                  diagnostics,
                  functions.ToImmutableArray(),
                  variables.ToImmutableArray(),
                  statements.ToImmutableArray())
        { }

        public BoundGlobalScope(
            BoundGlobalScope previous,
            DiagnosticBag diagnostics,
            ImmutableArray<FunctionSymbol> functions,
            ImmutableArray<VariableSymbol> variables,
            ImmutableArray<BoundStatement> statements)
        {
            Previous = previous;

            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            Functions = functions;
            Variables = variables;
            Statements = statements;
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Diagnostics { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}