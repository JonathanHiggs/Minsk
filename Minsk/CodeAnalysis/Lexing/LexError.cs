using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Lexing
{
    public class LexError : Diagnostic
    {
        public LexError(LexErrorKind errorKind, TextLocation location, string text, string message)
            : base(location, message)
        {
            ErrorKind = errorKind;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override DiagnosticKind Kind => DiagnosticKind.LexError;

        public LexErrorKind ErrorKind { get; }

        public string Text { get; }

        public override string ToString() => $"LexingError  {Message}";
    }
}