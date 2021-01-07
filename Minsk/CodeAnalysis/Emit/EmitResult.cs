using Minsk.CodeAnalysis.Diagnostics;

namespace Minsk.CodeAnalysis.Emit
{
    public sealed class EmitResult
    {
        public EmitResult(DiagnosticBag diagnostics)
        {
            Diagnostics = diagnostics;
        }

        public DiagnosticBag Diagnostics { get; }
    }
}
