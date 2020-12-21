using System;
using System.IO;

using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public abstract class SyntaxNode : Node<SyntaxNode, SyntaxKind>
    {
        public abstract string Text { get; }

        public TextSpan Span => TextSpan.FromBounds(FirstToken.Span.Start, LastToken.Span.End);

        public abstract LexToken FirstToken { get; }

        public abstract LexToken LastToken { get; }

        protected override string PrettyPrintText() => Text;

        protected override ConsoleColor PrettyPrintColorForKind(SyntaxKind kind)
        {
            return Kind switch {
                SyntaxKind.AssignmentExpression
                    => ConsoleColor.Green,

                SyntaxKind.BinaryExpression or SyntaxKind.UnaryExpression
                    => ConsoleColor.Cyan,

                SyntaxKind.NameExpression or SyntaxKind.LiteralExpression
                    => ConsoleColor.Magenta,

                _   => Console.ForegroundColor
            };
        }

        public override string ToString()
        {
            using var writer = new StringWriter();

            PrettyPrint(writer);
            return writer.ToString();
        }
    }
}