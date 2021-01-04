using System;
using System.IO;
using System.Linq;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Parsing;
using Minsk.IO;

namespace Minsk.Compiler
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: mkc <source-paths>");
                return;
            }

            if (args.Length > 1)
            {
                Console.Error.WriteLine($"error: only one path supported right now");
                return;
            }

            var path = args.Single();

            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"error: file '{path}' not found");
                return;
            }

            var text = File.ReadAllText(path);

            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = Compilation.Compile(syntaxTree);
            var result = compilation.Evaluate();

            if (!result.Diagnostics.Any())
            {
                Console.WriteLine(result.Value);
            }
            else
            {
                Console.Out.WriteDiagnostics(result.Diagnostics);
            }
        }
    }
}
