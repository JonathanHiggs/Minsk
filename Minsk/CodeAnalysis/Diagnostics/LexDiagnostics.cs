using System;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public sealed class LexDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal LexDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        public void InvalidNumber(TextSpan span, string text, string message)
            => bag.Report(new LexError(span, text, message));

        public void InvalidCharacters(TextSpan span, string text, string message)
            => bag.Report(new LexError(span, text, message));
    }
}