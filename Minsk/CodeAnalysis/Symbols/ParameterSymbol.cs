using System.IO;

using Minsk.CodeAnalysis.Lexing;
using Minsk.IO;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class ParameterSymbol : LocalVariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type)
            : base(name, isReadOnly: true, type)
        { }

        public override SymbolKind Kind => SymbolKind.Parameter;

        public override void WriteTo(TextWriter writer)
        {
            writer.WriteIdentifier(Name);
            writer.WritePunctuation(TokenKind.Colon);
            writer.WriteSpace();
            Type?.WriteTo(writer);
        }
    }
}