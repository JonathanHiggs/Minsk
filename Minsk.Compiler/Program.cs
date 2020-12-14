using System;
using System.Linq;

using Minsk.Compiler.Core;
using Minsk.Compiler.Diagnostic;
using Minsk.Compiler.Parsing;


namespace Minsk.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            REPL();
        }

        static void REPL()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.ToLower() == "clear")
                {
                    Console.Clear();
                    continue;
                }
                
                if (line.ToLower() == "exit")
                    return;

                var parser = new Parser(line);
                var tree = parser.Parse();

                if (tree.Errors.Any())
                {
                    Console.WriteLine("\n--- Errors");
                    var foreground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    foreach (var error in tree.Errors)
                        Console.WriteLine(error);

                    Console.ForegroundColor = foreground;
                }

                Console.WriteLine("\n--- Expression Tree");
                tree.Root.PrettyPrint();

                Console.WriteLine("\n--- Visual Tree");
                var visualTree = tree.ToVisualTree();
                visualTree.Print();

                if (!tree.Errors.Any())
                {
                    Console.WriteLine("--- Value");
                    if (Evaluator.Eval(tree.Root, out var result))
                        Console.WriteLine(result);
                }
                    
                Console.WriteLine();
            }
        }

        static void Test()
        {
            var line = "1 + 2";
            Console.WriteLine($"> {line}");

            var parser = new Parser(line);
            var tree = parser.ParseExpression();
            
            tree.ToVisualTree().Print();
            Console.WriteLine("Done");
        }

        static void Test2()
        {
            var settings = new VisualTreeSettings();

            var tree = 
                new BinaryVisualNode("Hello", "Node",
                    new BinaryVisualNode("Left", "Node",
                        new UnaryVisualNode("1", "Node",
                            new TerminalVisualNode("End", "Node", settings),
                            settings),
                        new BinaryVisualNode("Something really long to unbalance in a really really bad way", "Node",
                            new TerminalVisualNode("End", "Node", settings),
                            new TerminalVisualNode("End", "Node", settings),
                            settings),
                        settings),
                    new BinaryVisualNode("Right", "Node",
                        new TerminalVisualNode("2", "Node", settings),
                        new TerminalVisualNode("345", "Node", settings),
                        settings),
                    settings);

            tree.Print();

            Console.ReadLine();
        }
    }
}
