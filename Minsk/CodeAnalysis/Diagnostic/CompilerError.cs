using System;

namespace Minsk.CodeAnalysis.Diagnostic
{
    public abstract class CompilerError
    { 
        protected CompilerError(string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public string Message { get; }
    }
}