using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Text;
using Minsk.IO;

namespace Minsk.Interactive
{
    internal sealed class MinskRepl : Repl
    {
        private static readonly Compilation emptyCompilation = Compilation.CreateScript(null);
        private Compilation previous;

        private bool loadingSubmission;
        private bool showTree;
        private bool showProgram;

        private readonly Dictionary<VariableSymbol, object> variables
            = new Dictionary<VariableSymbol, object>();

        public MinskRepl()
            : base()
        {
            LoadSubmissions();
        }

        protected override object RenderLine(IReadOnlyList<string> lines, int lineIndex, object state)
        {
            RenderLine(lines[lineIndex]);

            return state;
        }

        private void RenderLine(string line)
        {
            var tokens = Lexer.Lex(SourceText.From(line), null).TakeWhile(t => t.Kind != TokenKind.EoF);
            foreach (var token in tokens)
            {
                var isKeyword = token.Kind.IsKeyword();
                var isNumber = token.Kind == TokenKind.Number;
                var isString = token.Kind == TokenKind.String;
                var isIdentifier = token.Kind == TokenKind.Identifier;

                if (isKeyword)
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (isNumber)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (isIdentifier)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (isString)
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                else
                    Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write(token.Text);
                Console.ResetColor();
            }
        }

        //protected override object RenderLine(IReadOnlyList<string> lines, int lineIndex, object state)
        //{
        //    SyntaxTree syntaxTree;

        //    if (state == null)
        //    {
        //        var text = string.Join(Environment.NewLine, lines);
        //        syntaxTree = SyntaxTree.Parse(text);
        //    }
        //    else
        //    {
        //        syntaxTree = (SyntaxTree)state;
        //    }

        //    var lineSpan = syntaxTree.Source.Lines[lineIndex].Span;
        //    var classifiedSpans = Classifier.Classify(syntaxTree, lineSpan);

        //    foreach (var classifiedSpan in classifiedSpans)
        //    {
        //        var classifiedText = syntaxTree.Source.ToString(classifiedSpan.Span);

        //        switch (classifiedSpan.Classification)
        //        {
        //            case Classification.Keyword:
        //                Console.ForegroundColor = ConsoleColor.Blue;
        //                break;

        //            case Classification.Identifier:
        //                Console.ForegroundColor = ConsoleColor.DarkYellow;
        //                break;

        //            case Classification.Number:
        //                Console.ForegroundColor = ConsoleColor.Cyan;
        //                break;

        //            case Classification.String:
        //                Console.ForegroundColor = ConsoleColor.Magenta;
        //                break;

        //            case Classification.Comment:
        //                Console.ForegroundColor = ConsoleColor.Green;
        //                break;

        //            case Classification.Text:
        //            default:
        //                Console.ForegroundColor = ConsoleColor.DarkGray;
        //                break;
        //        }

        //        Console.Write(classifiedText);
        //        Console.ResetColor();
        //    }

        //    return syntaxTree;
        //}

        [MetaCommand("exit", "Exits the REPL")]
        private void EvaluateExit()
        {
            Environment.Exit(0);
        }

        [MetaCommand("cls", "Clears the screen")]
        private void EvaluateCls()
        {
            Console.Clear();
        }

        [MetaCommand("reset", "Clears all previous submissions")]
        private void EvaluateReset()
        {
            previous = null;
            variables.Clear();
            ClearSubmissions();
        }

        [MetaCommand("delete", "Deletes saves submissions")]
        private void EvaluateDelete()
        {
            ClearSubmissions();
        }

        [MetaCommand("showTree", "Shows the parse tree")]
        private void EvaluateShowTree()
        {
            showTree = !showTree;
            Console.WriteLine(showTree ? "Showing parse trees" : "Not showing parse trees");
        }

        [MetaCommand("showProgram", "Shows the bound tree")]
        private void EvaluateShowProgram()
        {
            showProgram = !showProgram;
            Console.WriteLine(showProgram ? "Showing bound tree" : "Not showing bound tree");
        }

        [MetaCommand("load", "Loads a script file")]
        private void EvaluateLoad(string path)
        {
            path = Path.GetFullPath(path);

            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"error: file does not exist '{path}'");
                Console.ResetColor();
                return;
            }

            var text = File.ReadAllText(path);
            EvaluateSubmittion(text);
        }

        [MetaCommand("ls", "Lists all symbols")]
        private void EvaluateLs()
        {
            var compilation = previous ?? emptyCompilation;
            var symbols = compilation.Symbols.OrderBy(s => s.Kind).ThenBy(s => s.Name);

            foreach (var symbol in symbols)
            {
                symbol.WriteTo(Console.Out);
                Console.WriteLine();
            }
        }

        [MetaCommand("dump", "Shows the bound tree for a given function")]
        private void EvaluateDump(string functionName)
        {
            var compilation = previous ?? emptyCompilation;
            var symbol = compilation.Symbols.OfType<FunctionSymbol>().SingleOrDefault(f => f.Name == functionName);

            if (symbol is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"error: function '{functionName}' does not exist");
                Console.ResetColor();
                return;
            }

            compilation.EmitTree(symbol, Console.Out);
        }

        protected override bool IsCompleteSubmission(string text)
        {
            //if (string.IsNullOrEmpty(text))
            //    return true;

            //var lastTwoLinesAreBlank =
            //    text.Split(Environment.NewLine)
            //        .Reverse()
            //        .TakeWhile(s => string.IsNullOrEmpty(s))
            //        .Take(2)
            //        .Count() == 2;

            //if (lastTwoLinesAreBlank)
            //    return true;

            //var syntaxTree = SyntaxTree.Parse(text);

            //var lastMember = syntaxTree.Root.Members.Last().LastToken;
            //if (lastMember is null || lastMember.IsMissing)
            //    return false;

            return true;
        }

        protected override void EvaluateSubmittion(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            Compilation compilation = Compilation.CreateScript(previous, syntaxTree);

            if (showTree)
                syntaxTree.Root.PrettyPrint(Console.Out);

            if (showProgram)
            {
                if (showTree)
                    Console.WriteLine();

                compilation.EmitTree(Console.Out);
            }

            if (showTree || showProgram)
                Console.WriteLine();

            var result = compilation.Evaluate(variables);
            Console.Out.WriteDiagnostics(result.Diagnostics);

            if (!result.Diagnostics.Any())
            {
                if (result.Value is not null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }

                previous = compilation;

                //SaveSubmission(text);
            }
        }

        private static string GetSubmissionsDirectory()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var submissionsDirectory = Path.Combine(localAppData, "Minsk", "Submissions");
            return submissionsDirectory;
        }

        private void LoadSubmissions()
        {
            var submissionsDirectory = GetSubmissionsDirectory();
            if (!Directory.Exists(submissionsDirectory))
                return;

            var files = Directory.GetFiles(submissionsDirectory).OrderBy(f => f).ToArray();
            if (!files.Any())
                return;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Loaded {files.Length} submission{(files.Any() ? "s" : "")}");
            Console.ResetColor();

            loadingSubmission = true;

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                EvaluateSubmittion(text);
            }

            loadingSubmission = false;
        }

        private static void ClearSubmissions()
        {
            var dir = GetSubmissionsDirectory();
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }

        private void SaveSubmission(string text)
        {
            if (loadingSubmission)
                return;

            var submissionsDirectory = GetSubmissionsDirectory();
            Directory.CreateDirectory(submissionsDirectory);
            var count = Directory.GetFiles(submissionsDirectory).Length;
            var name = $"submission{count:00000}.mks";
            var filename = Path.Combine(submissionsDirectory, name);
            File.WriteAllText(filename, text);
        }
    }
}
