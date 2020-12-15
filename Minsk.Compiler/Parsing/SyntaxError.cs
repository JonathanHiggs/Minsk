using System;

using Minsk.Compiler.Core;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public class SyntaxError : CompilerError
    {
        public SyntaxError(SyntaxToken token, string message)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public SyntaxToken Token { get; }
        public override string Message { get; }
        

        public override string ToString()
            => $"SyntaxError  {Token.Position}  \"{Token.Text}\"  {Message}";
    }
}