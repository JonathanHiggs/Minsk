using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.Compiler.Diagnostic;
using Minsk.Compiler.Lexing;

namespace Minsk.Compiler.Parsing
{
    public class SyntaxTree
    {
        public SyntaxTree(Expression root, LexToken eofToken, IEnumerable<CompilerError> errors)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            EoFToken = eofToken ?? throw new ArgumentNullException(nameof(eofToken));
            Errors = errors?.ToList() ?? throw new ArgumentNullException(nameof(errors));
        }

        public static SyntaxTree Parse(string line)
        {
            var parser = new Parser(line);
            return parser.Parse();
        }

        public Expression Root { get; }

        public LexToken EoFToken { get; }

        public IReadOnlyList<CompilerError> Errors { get; }
    }
}