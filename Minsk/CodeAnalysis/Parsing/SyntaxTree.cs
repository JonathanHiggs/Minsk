using System;
using System.IO;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public sealed class SyntaxTree
    {
        public SyntaxTree(SourceText source, DiagnosticBag diagnostics)
        {
            Source = source
                ?? throw new ArgumentNullException(nameof(source));

            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));

            var parser = new Parser(this, source, diagnostics);
            Root = parser.ParseCompilationUnit();
        }

        public static SyntaxTree Load(string filename)
        {
            var text = File.ReadAllText(filename);
            var sourceText = SourceText.From(text, filename);
            return Parse(sourceText);
        }

        public static SyntaxTree Parse(string text, DiagnosticBag diagnostics = null)
            => Parse(SourceText.From(text), diagnostics);

        public static SyntaxTree Parse(SourceText source, DiagnosticBag diagnostics = null)
        {
            diagnostics ??= new DiagnosticBag();
            return new SyntaxTree(source, diagnostics);
        }

        public SourceText Source { get; }

        public CompilationUnit Root { get; }

        public DiagnosticBag Diagnostics { get; }
    }
}