using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Emit
{
    public sealed class EmitError : Diagnostic
    {
        public EmitError(string message)
            : base(TextLocation.Empty, message, DiagnosticWarningLevel.Error)
        { }

        public override DiagnosticKind Kind => DiagnosticKind.EmitError;

        public override string ToString() => $"EmitError  {Message}";
    }
}
