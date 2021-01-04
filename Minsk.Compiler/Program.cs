using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;
using Minsk.IO;

namespace Minsk.Compiler
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            args = new string[] { "D:\\dev\\lang\\minsk\\samples\\multi-file" };

            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: mkc <source-paths>");
                return;
            }

            var (filePaths, errorPaths) = GetFilePaths(args);

            if (errorPaths.Any())
            {
                foreach (var path in errorPaths)
                    Console.Error.WriteLine($"error: path '{path}' does not exist");
                return;
            }

            if (!filePaths.Any())
            {
                Console.Error.WriteLine($"error: no files found");
                return;
            }

            var diagnostics = new DiagnosticBag();
            var syntaxTrees =
                filePaths.Select(path => SourceText.Load(path))
                     .Select(source => SyntaxTree.Parse(source, diagnostics));

            var compilation = Compilation.Compile(diagnostics, syntaxTrees);
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

        private static (IEnumerable<string> FilePaths, IEnumerable<string> ErrorPaths) GetFilePaths(IEnumerable<string> paths)
        {
            var search = new Stack<string>(paths);
            var results = new SortedSet<string>();
            var errors = new List<string>();

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    var filePaths = Directory.EnumerateFiles(path, "*.mk", SearchOption.AllDirectories);
                    foreach (var filePath in filePaths)
                        results.Add(filePath);
                }
                else if (File.Exists(path))
                {
                    results.Add(path);
                }
                else
                {
                    errors.Add(path);
                }
            }

            return (results, errors);
        }
    }
}
