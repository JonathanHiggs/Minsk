using System;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;

namespace Minsk.CodeAnalysis.Parsing
{
    public class SyntaxTree
    {
        public SyntaxTree(Expression root, LexToken eofToken, DiagnosticBag diagnostics)
        {
            Root = root 
                ?? throw new ArgumentNullException(nameof(root));

            EoFToken = eofToken 
                ?? throw new ArgumentNullException(nameof(eofToken));

            Diagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public static SyntaxTree Parse(string line)
        {
            var diagnostics = new DiagnosticBag();
            var parser = new Parser(line, diagnostics);
            return parser.Parse();
        }

        public Expression Root { get; }

        public LexToken EoFToken { get; }

        public DiagnosticBag Diagnostics { get; }
    }
}