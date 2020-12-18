using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

using Minsk.CodeAnalysis.Text;

namespace Minsk.UnitTests.CodeAnalysis
{
    public sealed class AnnotatedText
    {
        public AnnotatedText(string text, string originalText, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            OriginalText = originalText;
            Spans = spans;
        }

        public string Text { get; }
        public string OriginalText { get; }
        public ImmutableArray<TextSpan> Spans { get; }

        public override string ToString()
            => OriginalText;

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);

            var textBuilder = new StringBuilder();
            var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
            var startStack = new Stack<int>();

            var position = 0;
            foreach (var c in text)
            {
                if (c == '[')
                {
                    startStack.Push(position);
                }
                else if (c == ']')
                {
                    if (startStack.Count == 0)
                        throw new ArgumentException($"Missing opening [ in text: {text}");

                    var start = startStack.Pop();
                    spanBuilder.Add(TextSpan.FromBounds(start, position));
                }
                else
                {
                    position++;
                    textBuilder.Append(c);
                }
            }

            if (startStack.Count != 0)
                throw new ArgumentException($"Missing closing ] in text: {text}");

            return new AnnotatedText(textBuilder.ToString(), text, spanBuilder.ToImmutable());
        }

        private static string Unindent(string text)
        {
            var lines = new List<string>();

            using (var stringReader = new StringReader(text))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            var minIndentation = int.MaxValue;
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    lines[i] = string.Empty;
                }
                else
                {
                    var indentation = line.Length - line.TrimStart().Length;
                    minIndentation = Math.Min(indentation, minIndentation);
                }
            }

            for (var i = 0; i < lines.Count; i++)
                if (lines[i] != string.Empty || lines[i].Length > minIndentation)
                    lines[i] = lines[i].Substring(minIndentation);

            while (lines.Count > 0 && lines[0].Length == 0)
                lines.RemoveAt(0);

            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
                lines.RemoveAt(lines.Count - 1);

            return string.Join(Environment.NewLine, lines);
        }
    }
}