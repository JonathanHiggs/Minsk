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
            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions,
            BoundBlockStatement statement)
        {
            Diagnostics = diagnostics;
            Functions = functions;
            Statement = statement;
        }

        public DiagnosticBag Diagnostics { get; }
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
    }
}