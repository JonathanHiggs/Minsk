using System.Collections.Generic;
using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Symbols
{
    internal static class FunctionSymbolHelpers
    { 
        internal static FunctionSymbolBuilder WithParameters(this string name, params ParameterSymbol[] parameters)
        {
            var builder = new FunctionSymbolBuilder(name);
            builder.Parameters(parameters);
            return builder;
        }

        internal static ParameterSymbol OfType<T>(this string name)
            => new ParameterSymbol(name, TypeSymbol.From<T>());

        internal static ParameterSymbol OfAnyType(this string name)
            => new ParameterSymbol(name, TypeSymbol.Any);

        internal static FunctionSymbol Returns<T>(this string name)
            => new FunctionSymbol(name, TypeSymbol.From<T>(), null, isBuiltin: true);

        internal static FunctionSymbol ReturnsAny(this string name)
            => new FunctionSymbol(name, TypeSymbol.Any, null, false);

        internal class FunctionSymbolBuilder
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
    }
}