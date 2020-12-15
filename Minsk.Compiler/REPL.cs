using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Parsing;

namespace Minsk.Compiler
{
    public class REPL
    {
        private bool shouldExit = false;

        private bool showTree = false;
        private bool showVisualTree = false;
        private bool showHeaders = false;
        private bool showErrorCarrot = false;

        private string prompt = ">";

        private ConsoleColor errorColor = ConsoleColor.DarkRed;

        private Dictionary<VariableSymbol, object> variables
            = new Dictionary<VariableSymbol, object>();

        public void Run()
        {
            while (!shouldExit)
            {
                Console.Write(prompt);
                Console.Write(" ");

                var line = Console.ReadLine();
                if (line.Length > 1 && line[0] == '#')
                    HandleCommand(line.Substring(1));
                else if (line.Length > 0)
                    Evaluate(line);
            }
        }

        public void HandleCommand(string line)
        {
            var sections = line.Split(' ');
            var command = sections[0];
            var args = sections.Skip(1);

            switch (command.ToLower())
            {
                case "clear":
                    Console.Clear();
                    return;

                case "exit":
                    shouldExit = true;
                    return;

                case "showtree":
                    showTree = true;
                    return;

                case "hidetree":
                    showTree = false;
                    return;

                case "showvisualtree":
                    showVisualTree = true;
                    return;

                case "hidevisualtree":
                    showVisualTree = false;
                    return;

                case "showtrees":
                    showTree = true;
                    showVisualTree = true;
                    return;

                case "hidetrees":
                    showTree = false;
                    showVisualTree = false;
                    return;

                default:
                    Console.WriteLine("Unknown command");
                    return;
            }
        }

        public void Evaluate(string line)
        {
            var tree = SyntaxTree.Parse(line);
            var compilation = new Compilation(tree);
            var result = compilation.Evaluate(variables);

            if (result.Diagnostics.Any())
            {
                if (showHeaders)
                    Console.WriteLine("\n--- Errors");

                Console.WriteLine();
                foreach (var diagnostic in result.Diagnostics)
                    WriteDiagnostic(diagnostic, line);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(result.Value);
                Console.WriteLine();
            }

            if (showTree)
            {
                if (showHeaders)
                    Console.WriteLine("--- Expression Tree");
                tree.Root.PrettyPrint();
            }

            if (showVisualTree)
            {
                if (showHeaders)
                    Console.WriteLine("\n--- Visual Tree");
                var visualTree = tree.ToVisualTree();
                visualTree.Print();
            }
        }

        public void WriteDiagnostic(Diagnostic diagnostic, string line)
        {
            Console.ForegroundColor = errorColor;
            Console.WriteLine(diagnostic);
            Console.ResetColor();

            var prefix = line.Substring(0, diagnostic.Source.Start);
            var error = line.Substring(diagnostic.Source.Start, diagnostic.Source.Length);
            var suffix = line.Substring(diagnostic.Source.End);

            Console.Write("    ");
            Console.Write(prefix);

            Console.ForegroundColor = errorColor;
            Console.Write(error);

            Console.ResetColor();
            Console.WriteLine(suffix);

            if (showErrorCarrot)
            {
                Console.Write(new string(' ', prefix.Length + 4));

                Console.ForegroundColor = errorColor;
                Console.WriteLine(new string('^', error.Length));
            }

            Console.ResetColor();
        }
    }
}