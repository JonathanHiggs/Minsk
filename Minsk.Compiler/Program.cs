using System;
using System.Linq;

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
                new BinaryVisualNode("Hello",
                    new BinaryVisualNode("Left", 
                        new UnaryVisualNode("1",
                            new TerminalVisualNode("End", settings),
                            settings),
                        new BinaryVisualNode("Something really long to unbalance in a really really bad way",
                            new TerminalVisualNode("End", settings),
                            new TerminalVisualNode("End", settings),
                            settings),
                        settings),
                    new BinaryVisualNode("Right",
                        new TerminalVisualNode("2", settings),
                        new TerminalVisualNode("345", settings),
                        settings),
                    settings);

            tree.Print();

            Console.ReadLine();
        }

        static void REPL()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line) || line == "Exit" || line == "exit")
                    return;

                var parser = new Parser(line);
                var tree = parser.Parse();

                if (tree.Errors.Any())
                {
                    var foreground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;

                    foreach (var error in tree.Errors)
                        Console.WriteLine(error);
                        
                    Console.ForegroundColor = foreground;
                }

                var visualTree = tree.ToVisualTree();

                visualTree.Print();
            }
        }
    }
}
