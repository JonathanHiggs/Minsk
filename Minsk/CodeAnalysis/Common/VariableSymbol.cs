using System;

namespace Minsk.CodeAnalysis.Common
{
    public sealed class VariableSymbol
    {
        public VariableSymbol(string name, bool isReadOnly, Type type)
        {
            Name = name;
            IsReadOnly = isReadOnly;
            Type = type;
        }

        public string Name { get; }
        public bool IsReadOnly { get; }
        public Type Type { get; }

        public override string ToString()
            => $"{Type?.Name ?? "Unknown"}:{Name ?? "Unknown"}";
    }
}