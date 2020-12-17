using System;
using System.Collections.Immutable;

using Minsk.CodeAnalysis.Diagnostics;

namespace Minsk.CodeAnalysis
{
    public sealed class EvaluationResult
    {
        public EvaluationResult(object value, ImmutableArray<Diagnostic> diagnostics)
        {
            Value = value;
            Diagnostics = diagnostics;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public object Value { get; }
    }
}