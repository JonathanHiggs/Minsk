using System.Collections;
using System.Collections.Generic;

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
        }

        public LexDiagnostics Lex { get; }
        public SyntaxDiagnostics Syntax { get; }
        public BindingDiagnostics Binding { get;}

        public void Report(Diagnostic diagnostic)
            => diagnostics.Add(diagnostic);

        public IEnumerator<Diagnostic> GetEnumerator() => diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => diagnostics.GetEnumerator();
    }
}