using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class SyntaxTree
    {
        private SyntaxTree(SourceText source, CompilationUnit root, DiagnosticBag diagnostics)
        {
            Source = source
                ?? throw new ArgumentNullException(nameof(source));

            Root = root
                ?? throw new ArgumentNullException(nameof(root));

            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public static SyntaxTree Parse(string text)
            => Parse(SourceText.From(text));

        public static SyntaxTree Parse(SourceText source)
        {
            var diagnostics = new DiagnosticBag();
            var parser = new Parser(source, diagnostics);
            var root = parser.ParseCompilationUnit();
            return new SyntaxTree(source, root, diagnostics);
        }

        public SourceText Source { get; }

        public CompilationUnit Root { get; }

        public DiagnosticBag Diagnostics { get; }
    }
}