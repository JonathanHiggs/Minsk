using System.Collections.Immutable;
using System.Linq;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name)
            : this(name, TypeSymbol.Void)
        { }

        public FunctionSymbol(string name, TypeSymbol returnType)
            : this(name, Enumerable.Empty<ParameterSymbol>().ToImmutableArray(), returnType)
        { }

        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType)
            : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
        }

        public override SymbolKind Kind => SymbolKind.Function;

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
    }
}