﻿using System;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol("?", isInternalType: true, isErrorType: true);
        public static readonly TypeSymbol Bool = new TypeSymbol("bool", isInternalType: true);
        public static readonly TypeSymbol Int = new TypeSymbol("int", isInternalType: true);
        public static readonly TypeSymbol Void = new TypeSymbol("void", isInternalType: true, isVoidType: true);
        public static readonly TypeSymbol String = new TypeSymbol("string", isInternalType: true);

        private TypeSymbol(
            string name, 
            bool isInternalType = false, 
            bool isErrorType = false, 
            bool isVoidType = false
        )
            : base(name)
        {
            IsInternalType = isInternalType;
            IsErrorType = isErrorType;
            IsVoidType = isVoidType;
        }

        public override SymbolKind Kind => SymbolKind.Type;

        public bool IsErrorType { get; }
        public bool IsVoidType { get; }
        public bool IsNotVoidType => !IsVoidType;
        public bool IsInternalType { get; }

        public static TypeSymbol FromValue(object value)
            => value switch {
                bool    => Bool,
                int     => Int,
                string  => String,
                _       => throw new NotImplementedException($"Type: {value.GetType().Name}")
            };

        public static TypeSymbol From<T>()
        {
            if      (typeof(T) == typeof(bool))     return Bool;
            else if (typeof(T) == typeof(int))      return Int;
            else if (typeof(T) == typeof(string))   return String;

            throw new Exception($"Unexpected type '{typeof(T).Name}'");
        }
    }
}