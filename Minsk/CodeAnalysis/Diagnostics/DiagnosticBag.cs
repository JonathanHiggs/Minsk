using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Diagnostics
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> diagnostics = new List<Diagnostic>();

        public DiagnosticBag()
        {
            Lex = new LexDiagnostics(this);
            Syntax = new SyntaxDiagnostics(this);
            Binding = new BindingDiagnostics(this);
            Emit = new EmitDiagnostics(this);
        }

        public LexDiagnostics Lex { get; }
        public SyntaxDiagnostics Syntax { get; }
        public BindingDiagnostics Binding { get; }
        public EmitDiagnostics Emit { get; }

        public void Report(Diagnostic diagnostic)
            => diagnostics.Add(diagnostic);

        public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => diagnostics.GetEnumerator();
    }
}