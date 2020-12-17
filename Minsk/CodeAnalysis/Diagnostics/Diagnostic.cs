using System;

using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public abstract class Diagnostic
    {
        protected Diagnostic(int start, int length, string message)
            : this(new TextSpan(start, length), message)
        { }

        protected Diagnostic(TextSpan span, string message)
        {
            Span = span;
            Message = message ?? string.Empty;
        }

        public abstract DiagnosticKind Kind { get; }
        public TextSpan Span { get; }
        public string Message { get; }
    }
}