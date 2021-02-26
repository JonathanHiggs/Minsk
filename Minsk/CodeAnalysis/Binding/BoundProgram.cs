using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundProgram : BoundNode
    {
        public BoundProgram(
            DiagnosticBag diagnostics,
            BoundProgram previous,
            FunctionSymbol mainFunction,
            FunctionSymbol scriptFunction,
            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
        {
            Diagnostics = diagnostics;
            Previous = previous;
            MainFunction = mainFunction;
            ScriptFunction = scriptFunction;
            Functions = functions;
        }

        public DiagnosticBag Diagnostics { get; }
        public BoundProgram Previous { get; }
        public FunctionSymbol MainFunction { get; }
        public FunctionSymbol ScriptFunction { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }

        public IEnumerable<(FunctionSymbol Symbol, BoundBlockStatement Body)> FunctionDefinitions
            => Functions.Select(kvp => (kvp.Key, kvp.Value));

        public override IEnumerable<BoundNode> Children => Functions.Values;

        public override BoundNodeKind Kind => BoundNodeKind.Program;

        protected override string PrettyPrintText() => string.Empty;
    }
}