using System;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Unknown = new TypeSymbol("unknown", isInternalType: true, isErrorType: true);
        public static readonly TypeSymbol Bool = new TypeSymbol("bool", isInternalType: true);
        public static readonly TypeSymbol Int = new TypeSymbol("int", isInternalType: true);
        public static readonly TypeSymbol String = new TypeSymbol("string", isInternalType: true);

        private TypeSymbol(string name, bool isInternalType = false, bool isErrorType = false)
            : base(name)
        {
            IsInternalType = isInternalType;
            IsErrorType = isErrorType;
        }

        public override SymbolKind Kind => SymbolKind.Type;

        public bool IsErrorType { get; }
        public bool IsInternalType { get; }

        public static TypeSymbol FromValue(object value)
            => value switch {
                bool    => Bool,
                int     => Int,
                string  => String,
                _       => throw new NotImplementedException($"Type: {value.GetType().Name}")
            };
    }
}