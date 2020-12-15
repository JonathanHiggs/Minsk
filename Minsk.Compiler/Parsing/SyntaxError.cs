using System;

using Minsk.Compiler.Diagnostic;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
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