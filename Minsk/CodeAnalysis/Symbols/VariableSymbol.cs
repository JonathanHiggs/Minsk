using System.IO;

using Minsk.CodeAnalysis.Lexing;
using Minsk.IO;

namespace Minsk.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        internal VariableSymbol(string name, bool isReadOnly, TypeSymbol type)
            : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
        }

        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }

        public override string ToString()
            => $"{Type?.Name ?? "Unknown"}:{Name ?? "Unknown"}";


        public override void WriteTo(TextWriter writer)
        {
            writer.WriteKeyword(IsReadOnly ? TokenKind.LetKeyword : TokenKind.VarKeyword);
            writer.WriteSpace();
            writer.WriteIdentifier(Name);
            writer.WriteSpace();
            writer.WritePunctuation(TokenKind.Colon);
            writer.WriteSpace();
            Type.WriteTo(writer);
        }
    }
}