using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public class SyntaxTree
    {
        public SyntaxTree(SourceText source, Expression root, LexToken eofToken, DiagnosticBag diagnostics)
        {
            Source = source;
            Root = root
                ?? throw new ArgumentNullException(nameof(root));

            EoFToken = eofToken
                ?? throw new ArgumentNullException(nameof(eofToken));

            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public static SyntaxTree Parse(string text)
        {
            var diagnostics = new DiagnosticBag();
            var source = SourceText.From(text);
            var parser = new Parser(source, diagnostics);
            return parser.Parse();
        }

        public static SyntaxTree Parse(SourceText source)
        {
            var diagnostics = new DiagnosticBag();
            var parser = new Parser(source, diagnostics);
            return parser.Parse();
        }

        public SourceText Source { get; }

        public Expression Root { get; }

        public LexToken EoFToken { get; }

        public DiagnosticBag Diagnostics { get; }
    }
}