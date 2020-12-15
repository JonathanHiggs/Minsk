using System;

using Minsk.CodeAnalysis.Common;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public abstract class Diagnostic
    { 
        protected Diagnostic(int start, int length, string message)
            : this(new TextSpan(start, length), message)
        { }

        protected Diagnostic(TextSpan source, string message)
        {
            Source = source;
            Message = message
                ?? throw new ArgumentNullException(nameof(message));
        }

        public TextSpan Source { get; }
        public string Message { get; }
    }
}