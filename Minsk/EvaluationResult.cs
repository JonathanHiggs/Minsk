using System;

using Minsk.CodeAnalysis.Diagnostics;

namespace Minsk.CodeAnalysis
{
    public sealed class EvaluationResult 
    {
        public EvaluationResult(DiagnosticBag diagnostics, object value)
        {
            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));
                
            Value = value;
        }

        public DiagnosticBag Diagnostics { get; }
        public object Value { get; }
    }
}