using System;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Diagnostics;

namespace Minsk.CodeAnalysis.Lexing
{
    public class LexError : Diagnostic
    {
        public LexError(LexErrorKind errorKind, TextSpan span, string text, string message)
            : base(span, message)
        {
            ErrorKind = errorKind;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override DiagnosticKind Kind => DiagnosticKind.LexError;

        public LexErrorKind ErrorKind { get; }

        public string Text { get; }

        public override string ToString()
            => $"LexingError  {Source.Start}  \"{Text}\"  {Message}";
    }
}