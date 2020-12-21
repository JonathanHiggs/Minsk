using System;

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
            var repl = new REPL();
            repl.Run();
        }
    }
}
