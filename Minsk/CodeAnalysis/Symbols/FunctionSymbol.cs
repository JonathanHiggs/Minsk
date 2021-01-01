using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.IO;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, FunctionDeclaration declaration, bool isBuiltin = false)
            : this(name, TypeSymbol.Void, declaration)
        { }

        public FunctionSymbol(
            string name, 
            TypeSymbol returnType, 
            FunctionDeclaration declaration, 
            bool isBuiltin = false
        )
            : this(name, Enumerable.Empty<ParameterSymbol>().ToImmutableArray(), returnType, declaration, isBuiltin)
        { }

        public FunctionSymbol(
            string name, 
            ImmutableArray<ParameterSymbol> parameters, 
            TypeSymbol returnType,
            FunctionDeclaration declaration,
            bool isBuiltin = false)
            : base(name)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Declaration = declaration;
            IsBuiltin = isBuiltin;
        }

        public override SymbolKind Kind => SymbolKind.Function;

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
        public FunctionDeclaration Declaration { get; }
        public bool IsBuiltin { get; }

        public override void WriteTo(TextWriter writer)
        {
            writer.WriteKeyword(TokenKind.FunctionKeyword);
            writer.WriteSpace();
            writer.WriteIdentifier(Name);
            writer.WritePunctuation(TokenKind.OpenParenthesis);

            for (var i = 0; i < Parameters.Length; i++)
            {
                Parameters[i].WriteTo(writer);
                if (i < Parameters.Length - 1)
                {
                    writer.WritePunctuation(TokenKind.Comma);
                    writer.WriteSpace();
                }
            }

            writer.WritePunctuation(TokenKind.CloseParenthesis);

            if (ReturnType.IsNotVoidType)
            {
                writer.WriteSpace();
                writer.WritePunctuation(TokenKind.Colon);
                writer.WriteSpace();
                ReturnType.WriteTo(writer);
            }
        }
    }
}