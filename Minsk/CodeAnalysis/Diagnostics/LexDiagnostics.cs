using System;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Diagnostics
{
    // ToDo: should this live in the Diagnostics or Lexing namespace
    public sealed class LexDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal LexDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        public void InvalidNumber(TextSpan span, string text, string message)
            => bag.Report(new LexError(LexErrorKind.InvalidNumber, span, text, message));

        public void InvalidCharacters(TextSpan span, string text, string message)
            => bag.Report(new LexError(LexErrorKind.InvalidCharacter, span, text, message));

        public void UnexpectedNullTerminator(TextSpan span, string text)
            => bag.Report(new LexError(
                LexErrorKind.UnexpectedNullTerminator, span, text, "Unexpected null terminator"));

        public void UnterminatedString(TextSpan span, string text)
            => bag.Report(new LexError(
                LexErrorKind.UnterminatedString, span, text, "Unterminated string"));
    }
}