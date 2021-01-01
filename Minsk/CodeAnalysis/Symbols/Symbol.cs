using System;
using System.IO;

namespace Minsk.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        private protected Symbol(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{nameof(name)} is null or white space");

            Name = name;
        }

        public string Name { get; }

        public abstract SymbolKind Kind { get; }

        public override string ToString() => Name;

        public virtual void WriteTo(TextWriter writer)
        {
            writer.WriteLine($"{Kind}: {Name}");
        }
    }
}