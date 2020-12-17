using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

namespace Minsk.CodeAnalysis.Parsing
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public SyntaxNode Parent { set; get; }

        public virtual IEnumerable<SyntaxNode> Children => ChildrenViaReflection();

        public abstract string Text { get; }

        public TextSpan Span => TextSpan.FromBounds(FirstToken.Span.Start, LastToken.Span.End);

        public abstract LexToken FirstToken { get; }

        public abstract LexToken LastToken { get; }

        public void PrettyPrint(TextWriter textWriter, string indent = "", bool isLast = true)
        {
            var toConsole = textWriter == Console.Out;

            var marker = isLast ? "└──" : "├──";

            if (toConsole)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                textWriter.Write(indent);
                textWriter.Write(marker);
                Console.ResetColor();
            }
            else
            {
                textWriter.Write(indent);
                textWriter.Write(marker);
            }


            if (toConsole)
            {
                switch (Kind)
                {
                    case SyntaxKind.AssignmentExpression:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;

                    case SyntaxKind.BinaryExpression:
                    case SyntaxKind.UnaryExpression:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;

                    case SyntaxKind.NameExpression:
                    case SyntaxKind.LiteralExpression:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;

                    default:
                        break;
                }

                textWriter.WriteLine($"{Kind}  {Text}");
                Console.ResetColor();
            }
            else
            {
                textWriter.WriteLine($"{Kind}  {Text}");
            }

            indent += isLast ? "   " : "│  ";

            var lastChild = Children.LastOrDefault();

            foreach (var child in Children)
                child.PrettyPrint(textWriter, indent, child == lastChild);
        }

        public override string ToString()
        {
            using var writer = new StringWriter();

            PrettyPrint(writer);
            return writer.ToString();
        }

        private IEnumerable<SyntaxNode> ChildrenViaReflection()
        {
            // This should work because it is iterating the properties as they are defined in the
            // metadata, as long as the implementing class members are in the correct order in source

            var properties =
                GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name != nameof(Parent));

            foreach (var property in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = property.GetValue(this) as SyntaxNode;
                    yield return child;
                }
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = property.GetValue(this) as IEnumerable<SyntaxNode>;
                    foreach (var child in children)
                        yield return child;
                }
            }
        }
    }
}