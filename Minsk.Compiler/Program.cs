using System;
using Minsk.Compiler.Diagnostic;

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
            var tree = parser.Parse();
            
            tree.ToVisualTree().Print();
            Console.WriteLine("Done");
        }

        static void Test2()
        {
            var settings = new VisualTreeSettings();
            var tree = 
                new BinaryVisualNode("Hello",
                    new BinaryVisualNode("Left", 
                        new UnaryVisualNode("Child",
                            new TerminalVisualNode("End", settings),
                            settings),
                        new TerminalVisualNode("Something really long to unbalance in a really really bad way", settings),
                        settings),
                    new BinaryVisualNode("Right",
                        new TerminalVisualNode("2", settings),
                        new TerminalVisualNode("345", settings),
                        settings),
                    settings);

            tree.Print();
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

                var visualTree = tree.ToVisualTree();

                visualTree.Print();
            }
        }
    }
}
