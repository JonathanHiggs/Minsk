using System;

using Minsk.CodeAnalysis.Diagnostic;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public class SyntaxError : CompilerError
    {
        public SyntaxError(LexToken token, string message)
            : base(message)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public LexToken Token { get; }
        
        public override string ToString()
            => $"SyntaxError  {Token.Position}  \"{Token.Text}\"  {Message}";
    }
}