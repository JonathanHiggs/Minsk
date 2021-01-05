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
            FunctionSymbol mainFunction,
            FunctionSymbol scriptFunction,
            IEnumerable<FunctionSymbol> functions,
            IEnumerable<VariableSymbol> variables,
            IEnumerable<BoundStatement> statements
        ) : this(
            previous,
            diagnostics,
            mainFunction,
            scriptFunction,
            functions.ToImmutableArray(),
            variables.ToImmutableArray(),
            statements.ToImmutableArray())
        { }

        public BoundGlobalScope(
            BoundGlobalScope previous,
            DiagnosticBag diagnostics,
            FunctionSymbol mainFunction,
            FunctionSymbol scriptFunction,
            ImmutableArray<FunctionSymbol> functions,
            ImmutableArray<VariableSymbol> variables,
            ImmutableArray<BoundStatement> statements)
        {
            Previous = previous;

            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            MainFunction = mainFunction;
            ScriptFunction = scriptFunction;
            Functions = functions;
            Variables = variables;
            Statements = statements;
        }

        public BoundGlobalScope Previous { get; }
        public DiagnosticBag Diagnostics { get; }

        public FunctionSymbol MainFunction { get; }
        public FunctionSymbol ScriptFunction { get; }

        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}