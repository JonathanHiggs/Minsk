using System.Collections.Generic;
using System.Collections.Immutable;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundProgram : BoundNode
    {
        public BoundProgram(
            DiagnosticBag diagnostics,
            BoundProgram previous,
            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions,
            BoundBlockStatement statement)
        {
            Diagnostics = diagnostics;
            Previous = previous;
            Functions = functions;
            Statement = statement;
        }

        public DiagnosticBag Diagnostics { get; }
        public BoundProgram Previous { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public BoundBlockStatement Statement { get; }

        public override IEnumerable<BoundNode> Children
        {
            get
            {
                foreach (var function in Functions)
                    yield return function.Value;
                yield return Statement;
            }
        }

        public override BoundNodeKind Kind => BoundNodeKind.Program;

        protected override string PrettyPrintText() => string.Empty;

        public IEnumerable<(FunctionSymbol Symbol, BoundBlockStatement Body)> AllFunctions()
        {
            var seenFunctions = new HashSet<string>();

            var program = this;
            while (program is not null)
            {
                foreach (var kvp in program.Functions)
                {
                    if (seenFunctions.Add(kvp.Key.Name))
                        yield return (kvp.Key, kvp.Value);
                }

                program = program.Previous;
            }
        }

        public (BoundBlockStatement Body, bool Success) TryLookupFunction(FunctionSymbol symbol)
        {
            var program = this;
            while (program is not null)
            {
                if (program.Functions.TryGetValue(symbol, out var body))
                    return (body, true);

                program = program.Previous;
            }

            return (null, false);
        }
    }
}