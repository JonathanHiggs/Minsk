using System;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Diagnostics
{
    // ToDo: should this live in the Diagnostics or Parsing namespace
    public sealed class SyntaxDiagnostics
    {
        private readonly DiagnosticBag bag;

        internal SyntaxDiagnostics(DiagnosticBag bag)
            => this.bag = bag ?? throw new ArgumentNullException(nameof(bag));

        public void UnexpectedToken(LexToken token, string message)
            => bag.Report(new SyntaxError(token, token.Location, message));
    }
}