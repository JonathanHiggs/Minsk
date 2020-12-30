using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Minsk.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = "Print".Function("text".OfType(TypeSymbol.String));

        public static readonly FunctionSymbol Input = "Input".Function(TypeSymbol.String);

        public static readonly FunctionSymbol Rand = "Rand".Function(TypeSymbol.Int);

        internal static IEnumerable<FunctionSymbol> All
            => typeof(BuiltinFunctions)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(FunctionSymbol))
                .Select(f => (FunctionSymbol)f.GetValue(null));

        #region Helpers

        private static ParameterSymbol OfType(this string name, TypeSymbol type)
            => new ParameterSymbol(name, type);

        private static FunctionSymbol Function(this string name, params ParameterSymbol[] parameters)
            => new FunctionSymbol(name, parameters.ToImmutableArray(), TypeSymbol.Void);

        private static FunctionSymbol Function(this string name, TypeSymbol returnType, params ParameterSymbol[] parameters)
            => new FunctionSymbol(name, parameters.ToImmutableArray(), returnType);

        #endregion
    }
}