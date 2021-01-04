using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public class SyntaxError : Diagnostic
    {
        public SyntaxError(LexToken token, TextLocation location, string message)
            : base(location, message)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        // ToDo: Add SyntexErrorKind ErrorKind

        public override DiagnosticKind Kind => DiagnosticKind.SyntaxError;

        public LexToken Token { get; }

        public override string ToString() => $"SyntaxError  {Message}";
    }
}