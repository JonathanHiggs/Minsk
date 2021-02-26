using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minsk.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        //public static readonly FunctionSymbol Print = "Print".WithParameters("text".OfAnyType());
        public static readonly FunctionSymbol Print = "Print".WithParameters("text".OfType<string>());

        public static readonly FunctionSymbol Input = "Input".Returns<string>();

        public static readonly FunctionSymbol Rand = "Rand".WithParameters("value".OfType<int>()).Returns<int>();

        internal static IEnumerable<FunctionSymbol> All
            => typeof(BuiltinFunctions)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(FunctionSymbol))
                .Select(f => (FunctionSymbol)f.GetValue(null));
    }
}