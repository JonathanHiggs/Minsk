using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.IO
{
    public static class TextWriterExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out)
                return !Console.IsOutputRedirected;

            if (writer == Console.Error)
                return !Console.IsErrorRedirected && !Console.IsOutputRedirected;

            //if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
            //    return true;

            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole())
                Console.ForegroundColor = color;
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsole())
                Console.ResetColor();
        }

        public static void WriteKeyword(this TextWriter writer, TokenKind kind)
        {
            var text = kind.GetText();
            Debug.Assert(kind.IsKeyword() && text != null);

            writer.WriteKeyword(text);
        }

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Cyan);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteSpace(this TextWriter writer)
        {
            writer.WritePunctuation(" ");
        }

        public static void WritePunctuation(this TextWriter writer, TokenKind kind)
        {
            var text = kind.GetText();
            Debug.Assert(text != null);

            writer.WritePunctuation(text);
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteDiagnostics(
            this TextWriter writer,
            IEnumerable<Diagnostic> diagnostics,
            SourceText source)
        {
            foreach (var diagnostic in diagnostics)
            {
                var lineIndex = source.LineIndexOf(diagnostic.Span.Start);
                var line = source.Lines[lineIndex];

                var lineNumber = lineIndex + 1;
                var lineCharacter = diagnostic.Span.Start - line.Start + 1;

                var prefix = source.ToString(line.Start, diagnostic.Span.Start - line.Start);
                var error = source.ToString(diagnostic.Span.Start, diagnostic.Span.Length);
                var suffix = source.ToString(diagnostic.Span.End, line.End - diagnostic.Span.End);

                writer.SetForeground(ConsoleColor.DarkRed);

                if (!string.IsNullOrWhiteSpace(source.FileName))
                    writer.Write($"{source.FileName} ");
                writer.Write($"({lineNumber}, {lineCharacter}): ");
                writer.WriteLine(diagnostic);
                writer.ResetColor();

                writer.Write("    ");
                writer.Write(prefix);

                writer.SetForeground(ConsoleColor.DarkRed);
                writer.Write(error);

                writer.ResetColor();
                writer.WriteLine(suffix);
                writer.ResetColor();
            }
        }

        //public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics)
        //{
        //    foreach (var diagnostic in diagnostics.Where(d => d.Location.Text == null))
        //    {
        //        var messageColor = diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
        //        writer.SetForeground(messageColor);
        //        writer.WriteLine(diagnostic.Message);
        //        writer.ResetColor();
        //    }

        //    foreach (var diagnostic in diagnostics.Where(d => d.Location.Text != null)
        //                                          .OrderBy(d => d.Location.FileName)
        //                                          .ThenBy(d => d.Location.Span.Start)
        //                                          .ThenBy(d => d.Location.Span.Length))
        //    {
        //        var text = diagnostic.Location.Text;
        //        var fileName = diagnostic.Location.FileName;
        //        var startLine = diagnostic.Location.StartLine + 1;
        //        var startCharacter = diagnostic.Location.StartCharacter + 1;
        //        var endLine = diagnostic.Location.EndLine + 1;
        //        var endCharacter = diagnostic.Location.EndCharacter + 1;

        //        var span = diagnostic.Location.Span;
        //        var lineIndex = text.GetLineIndex(span.Start);
        //        var line = text.Lines[lineIndex];

        //        writer.WriteLine();

        //        var messageColor = diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
        //        writer.SetForeground(messageColor);
        //        writer.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
        //        writer.WriteLine(diagnostic);
        //        writer.ResetColor();

        //        var prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
        //        var suffixSpan = TextSpan.FromBounds(span.End, line.End);

        //        var prefix = text.ToString(prefixSpan);
        //        var error = text.ToString(span);
        //        var suffix = text.ToString(suffixSpan);

        //        writer.Write("    ");
        //        writer.Write(prefix);

        //        writer.SetForeground(ConsoleColor.DarkRed);
        //        writer.Write(error);
        //        writer.ResetColor();

        //        writer.Write(suffix);

        //        writer.WriteLine();
        //    }

        //    writer.WriteLine();
        //}
    }
}
