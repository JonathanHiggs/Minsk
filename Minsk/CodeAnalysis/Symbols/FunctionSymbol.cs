using System.Collections.Immutable;
using System.Linq;

using Minsk.CodeAnalysis.Parsing;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, FunctionDeclaration declaration, bool isBuiltin = false)
            : this(name, TypeSymbol.Void, declaration)
        { }

        public FunctionSymbol(
            string name, 
            TypeSymbol returnType, 
            FunctionDeclaration declaration, 
            bool isBuiltin = false
        )
            : this(name, Enumerable.Empty<ParameterSymbol>().ToImmutableArray(), returnType, declaration, isBuiltin)
        { }

        public FunctionSymbol(
            string name, 
            ImmutableArray<ParameterSymbol> parameters, 
            TypeSymbol returnType,
            FunctionDeclaration declaration,
            bool isBuiltin = false)
            : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Declaration = declaration;
            IsBuiltin = isBuiltin;
        }

        public override SymbolKind Kind => SymbolKind.Function;

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
        public FunctionDeclaration Declaration { get; }
        public bool IsBuiltin { get; }
    }
}