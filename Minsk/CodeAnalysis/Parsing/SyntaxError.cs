using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public class SyntaxError : Diagnostic
    {
        public SyntaxError(LexToken token, string message)
            : base(token.Position, token.Text?.Length ?? 0, message)
        {
            Token = token
                ?? throw new ArgumentNullException(nameof(token));
        }

        public LexToken Token { get; }
        
        public override string ToString()
            => $"SyntaxError  {Source.Start}  \"{Token.Text ?? ""}\"  {Message}";
    }
}