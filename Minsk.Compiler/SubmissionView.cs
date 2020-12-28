using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Minsk.Compiler
{
    internal sealed class SubmissionView
    {
        private readonly LineRenderHandler lineRenderer;
        private readonly ObservableCollection<string> submissionDocument;
        private int cursorTop;
        private int renderedLineCount;
        private int currentLine;
        private int currentCharacter;

        public SubmissionView(
            LineRenderHandler lineRenderer,
            ObservableCollection<string> submissionDocument)
        {
            this.lineRenderer = lineRenderer
                ?? throw new ArgumentNullException(nameof(lineRenderer));

            this.submissionDocument = submissionDocument
                ?? throw new ArgumentNullException(nameof(submissionDocument));

            this.submissionDocument.CollectionChanged += SubmissionDocumentChanged;

            cursorTop = Console.CursorTop;

            Render();
        }

        public int CurrentLine
        {
            get => currentLine;
            set
            {
                if (currentLine == value)
                    return;

                currentLine = value;
                currentCharacter = Math.Min(submissionDocument[currentLine].Length, currentCharacter);

                UpdateCursorPosition();
            }
        }

        public int CurrentCharacter
        {
            get => currentCharacter;
            set
            {
                if (currentCharacter == value)
                    return;

                currentCharacter = value;
                UpdateCursorPosition();
            }
        }

        private void SubmissionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e)
            => Render();

        private void Render()
        {
            Console.CursorVisible = false;

            var lineCount = 0;
            var state = (object)null;

            foreach (var line in submissionDocument)
            {
                if (cursorTop + lineCount >= Console.WindowHeight)
                {
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.WriteLine();
                    if (cursorTop > 0)
                        cursorTop--;
                }

                Console.SetCursorPosition(0, cursorTop + lineCount);
                Console.ForegroundColor = ConsoleColor.Green;

                if (lineCount == 0)
                    Console.Write("» ");
                else
                    Console.Write("· ");

                Console.ResetColor();

                state = lineRenderer(submissionDocument, lineCount, state);
                Console.Write(new string(' ', Console.WindowWidth - line.Length - 2));
                lineCount++;
            }

            var numberOfBlankLines = renderedLineCount - lineCount;
            if (numberOfBlankLines > 0)
            {
                var blankLine = new string(' ', Console.WindowWidth);
                for (var i = 0; i < numberOfBlankLines; i++)
                {
                    Console.SetCursorPosition(0, cursorTop + lineCount + i);
                    Console.WriteLine(blankLine);
                }
            }

            renderedLineCount = lineCount;

            Console.CursorVisible = true;
            UpdateCursorPosition();
        }

        private void UpdateCursorPosition()
        {
            Console.CursorTop = cursorTop + currentLine;
            Console.CursorLeft = 2 + currentCharacter;
        }
    }
}
