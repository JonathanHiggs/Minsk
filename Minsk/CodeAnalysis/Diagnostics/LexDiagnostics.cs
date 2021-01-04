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


        public void InvalidNumber(TextLocation location, string text, string message)
            => bag.Report(new LexError(LexErrorKind.InvalidNumber, location, text, message));

        public void InvalidCharacters(TextLocation location, string text, string message)
            => bag.Report(new LexError(LexErrorKind.InvalidCharacter, location, text, message));

        public void UnexpectedNullTerminator(TextLocation location, string text)
            => bag.Report(new LexError(
                LexErrorKind.UnexpectedNullTerminator, location, text, "Unexpected null terminator"));

        public void UnterminatedString(TextLocation location, string text)
            => bag.Report(new LexError(
                LexErrorKind.UnterminatedString, location, text, "Unterminated string"));
    }
}