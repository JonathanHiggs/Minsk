using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }


        public bool TryDeclareFunction(FunctionSymbol function) => TryDeclareSymbol(function);

        public bool TryDeclareVariable(VariableSymbol variable) => TryDeclareSymbol(variable);

        private bool TryDeclareSymbol<TSymbol>(TSymbol symbol)
            where TSymbol : Symbol
        {
            if (symbols.ContainsKey(symbol.Name))
                return false;

            symbols.Add(symbol.Name, symbol);
            return true;
        }

        public (bool Success, Symbol Symbol) TryLookupSymbol(string name)
        {
            if (symbols.TryGetValue(name, out var symbol))
                return (true, symbol);

            return Parent?.TryLookupSymbol(name) ?? (false, null);
        }

        public IEnumerable<FunctionSymbol> Functions => DeclaredSymbols<FunctionSymbol>();

        public IEnumerable<VariableSymbol> Variables => DeclaredSymbols<VariableSymbol>();

        private IEnumerable<TSymbol> DeclaredSymbols<TSymbol>()
            where TSymbol : Symbol
        {
            return symbols.Values.OfType<TSymbol>();
        }
    }
}