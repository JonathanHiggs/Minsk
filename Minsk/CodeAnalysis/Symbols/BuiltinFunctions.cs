using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Minsk.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = "Print".WithParameters("text".OfType<string>());

        public static readonly FunctionSymbol Input = "Input".Returns<string>();

        public static readonly FunctionSymbol Rand = "Rand".WithParameters("value".OfType<int>()).Returns<int>();

        internal static IEnumerable<FunctionSymbol> All
            => typeof(BuiltinFunctions)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(FunctionSymbol))
                .Select(f => (FunctionSymbol)f.GetValue(null));

        #region Helpers

        private static FunctionSymbolBuilder WithParameters(this string name, params ParameterSymbol[] parameters)
        {
            var builder = new FunctionSymbolBuilder(name);
            builder.Parameters(parameters);
            return builder;
        }

        private static ParameterSymbol OfType<T>(this string name)
            => new ParameterSymbol(name, TypeSymbol.From<T>());

        private static FunctionSymbol Returns<T>(this string name)
            => new FunctionSymbol(name, TypeSymbol.From<T>(), null, isBuiltin: true);

        private class FunctionSymbolBuilder
        {
            private string name;
            private TypeSymbol returnType = TypeSymbol.Void;
            private List<ParameterSymbol> parameters = new List<ParameterSymbol>();

            public FunctionSymbolBuilder(string name)
            {
                this.name = name;
            }

            public static implicit operator FunctionSymbol(FunctionSymbolBuilder builder)
                => new FunctionSymbol(
                    builder.name, 
                    builder.parameters.ToImmutableArray(), 
                    builder.returnType,
                    null,
                    isBuiltin: true);

            public FunctionSymbolBuilder Parameters(params ParameterSymbol[] parameters)
            {
                foreach (var parameter in parameters)
                    this.parameters.Add(parameter);

                return this;
            }

            public FunctionSymbolBuilder Returns<T>()
            {
                returnType = TypeSymbol.From<T>();
                return this;
            }
        }

        #endregion
    }
}