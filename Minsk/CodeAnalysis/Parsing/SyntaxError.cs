using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public class SyntaxError : Diagnostic
    {
        public SyntaxError(LexToken token, string message)
            : base(token.Span, message)
        {
            Token = token
                ?? throw new ArgumentNullException(nameof(token));
        }

        public override DiagnosticKind Kind => DiagnosticKind.SyntaxError;

        public LexToken Token { get; }

        public override string ToString()
            => $"SyntaxError  {Source.Start}  \"{Token.Text ?? ""}\"  {Message}";
    }
}