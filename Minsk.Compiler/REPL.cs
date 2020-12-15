using System;
using System.Linq;
using Minsk.Compiler.Binding;
using Minsk.Compiler.Core;
using Minsk.Compiler.Parsing;

namespace Minsk.Compiler
{
    public class REPL
    {
        private bool shouldExit = false;
        private bool showTree = true;
        private bool showVisualTree = true;
        private string prompt = ">";

        private ConsoleColor foreground = ConsoleColor.White;
        //private ConsoleColor backround = ConsoleColor.Black;
        private ConsoleColor errorColor = ConsoleColor.DarkRed;

        public void Run()
        {
            while (!shouldExit)
            {
                Console.Write(prompt);
                Console.Write(" ");

                var line = Console.ReadLine();
                if (line.Length > 1 && line[0] == '#')
                    HandleCommand(line.Substring(1));
                else
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
            var binder = new Binder();
            var tree = SyntaxTree.Parse(line);
            var boundExpression = binder.BindExpression(tree.Root);

            var errors = tree.Errors.Concat(binder.Errors).ToList();

            if (errors.Any())
            {
                Console.ForegroundColor = errorColor;
                Console.WriteLine("\n--- Errors");

                foreach (var error in errors)
                    Console.WriteLine(error);

                Console.ForegroundColor = foreground;
            }
            else
            {
                if (Evaluator.Eval(boundExpression, out var result))
                    Console.WriteLine(result);
                Console.WriteLine();
            }

            if (showTree)
            {
                Console.WriteLine("--- Expression Tree");
                tree.Root.PrettyPrint();
            }

            if (showVisualTree)
            {
                Console.WriteLine("\n--- Visual Tree");
                var visualTree = tree.ToVisualTree();
                visualTree.Print();
            }
        }
    }
}