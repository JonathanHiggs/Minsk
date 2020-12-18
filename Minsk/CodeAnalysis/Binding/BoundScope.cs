using System.Collections.Generic;
using System.Collections.Immutable;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, VariableSymbol> variables 
            = new Dictionary<string, VariableSymbol>();

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        public ImmutableArray<VariableSymbol> DeclaredVariables 
            => variables.Values.ToImmutableArray();

        public (bool Success, VariableSymbol Variable) TryLookup(string name)
        {
            if (variables.TryGetValue(name, out var variable))
                return (true, variable);

            if (Parent is not null)
                return Parent.TryLookup(name);

            return (false, null);
        }

        public bool TryDeclare(VariableSymbol variable)
        {
            if (variables.ContainsKey(variable.Name))
                return false;

            variables.Add(variable.Name, variable);
            return true;
        }
    }
}