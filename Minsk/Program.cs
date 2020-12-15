using System;
using System.Linq;

using Minsk.CodeAnalysis.Diagnostic.Visualization;
using Minsk.CodeAnalysis.Parsing;


namespace Minsk.CodeAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            REPL();
        }

        static void REPL()
        {
            var repl = new REPL();
            repl.Run();
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
