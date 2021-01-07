using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Emit;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;
using Minsk.IO;

using Mono.Options;

namespace Minsk.Compiler
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var moduleName = string.Empty;
            var outputPath = string.Empty;
            var referencePaths = new List<string>();
            var sourcePaths = new List<string>();
            var helpRequested = false;

            var options = new OptionSet {
                "usage: mkc <source-paths> [options]",
                { "m=", "The name of the module", v => moduleName = v },
                { "r=", "The {path} of an assembly to reference", v => referencePaths.Add(v) },
                { "o=", "The output {path} of the assembly to create", v => outputPath = v },
                { "<>", v => sourcePaths.Add(v) },
                { "?|h|help", "Display the help", v => helpRequested = true }
            };

            options.Parse(args);

            if (helpRequested)
            {
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            var (filePaths, errorPaths) = GetFilePaths(sourcePaths);

            if (errorPaths.Any())
            {
                foreach (var path in errorPaths)
                    Console.Error.WriteLine($"error: path '{path}' does not exist");
                return 1;
            }

            if (!filePaths.Any())
            {
                Console.Error.WriteLine($"error: no source files found");
                return 1;
            }

            outputPath = InferOutputPath(outputPath, sourcePaths);
            moduleName = InferModuleName(moduleName, outputPath);

            if (!CheckReferencesOk(referencePaths))
                return 1;

            var diagnostics = new DiagnosticBag();
            var syntaxTrees =
                filePaths.Select(path => SourceText.Load(path))
                     .Select(source => SyntaxTree.Parse(source, diagnostics));

            var compilation = Compilation.Create(diagnostics, syntaxTrees);
            //var result = compilation.Evaluate();

            var emitted = compilation.Emit(moduleName, referencePaths, outputPath);

            if (emitted.Diagnostics.Any())
            {
                Console.Out.WriteDiagnostics(emitted.Diagnostics);
                return 1;
            }

            // Save?

            return 0;
        }

        private static string InferOutputPath(string outputPath, List<string> sourcePaths)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.ChangeExtension(sourcePaths[0], ".exe");
                Console.WriteLine($"Inferring output path as '{outputPath}'");
            }

            return outputPath;
        }

        private static string InferModuleName(string moduleName, string outputPath)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                moduleName = Path.GetFileNameWithoutExtension(outputPath);
                Console.WriteLine($"Inferring module name as '{moduleName}'");
            }

            return moduleName;
        }

        private static bool CheckReferencesOk(List<string> referencePaths)
        {
            var error = false;
            foreach (var path in referencePaths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' does not exist");
                    error = true;
                }
            }
            return !error;
        }

        private static (IEnumerable<string> FilePaths, IEnumerable<string> ErrorPaths) GetFilePaths(IEnumerable<string> paths)
        {
            var results = new SortedSet<string>();
            var errors = new List<string>();

            foreach (var path in paths)
            {
                //if (Directory.Exists(path))
                //{
                //    var filePaths = Directory.EnumerateFiles(path, "*.mk", SearchOption.AllDirectories);
                //    foreach (var filePath in filePaths)
                //        results.Add(filePath);
                //}
                if (File.Exists(path))
                    results.Add(path);
                else
                    errors.Add(path);
            }

            return (results, errors);
        }
    }
}
