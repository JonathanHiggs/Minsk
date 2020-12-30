using System.Collections.Generic;

using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private readonly Dictionary<string, FunctionSymbol> functions
            = new Dictionary<string, FunctionSymbol>();

        private readonly Dictionary<string, VariableSymbol> variables
            = new Dictionary<string, VariableSymbol>();

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        #region Functions

        public IEnumerable<FunctionSymbol> DeclaredFunctions => functions.Values;

        public (bool Success, FunctionSymbol Function) TryLookupFunction(string name)
        {
            if (functions.TryGetValue(name, out var function))
                return (true, function);

            if (Parent is not null)
                return Parent.TryLookupFunction(name);

            return (false, null);
        }

        public bool TryDeclareFunction(FunctionSymbol function)
        {
            if (functions.ContainsKey(function.Name))
                return false;

            functions.Add(function.Name, function);
            return true;
        }

        #endregion

        #region Variables

        public IEnumerable<VariableSymbol> DeclaredVariables => variables.Values;

        public (bool Success, VariableSymbol Variable) TryLookupVariable(string name)
        {
            if (variables.TryGetValue(name, out var variable))
                return (true, variable);

            if (Parent is not null)
                return Parent.TryLookupVariable(name);

            return (false, null);
        }

        public bool TryDeclareVariable(VariableSymbol variable)
        {
            if (variables.ContainsKey(variable.Name))
                return false;

            variables.Add(variable.Name, variable);
            return true;
        }

        #endregion
    }
}