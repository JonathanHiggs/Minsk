using System;

using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public sealed class LexDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal LexDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        public void InvalidNumber(int start, int length, string text, string message)
            => bag.Report(new LexError(start, length, text, message));

        public void UnexpectedCharacters(int start, int length, string text, string message)
            => bag.Report(new LexError(start, length, text, message));
    }
}