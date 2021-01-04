using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Parsing
{
    [TestFixture]
    public class ParserDiagnosticTests
    {
        [Test]
        public void Parse_WithParameterListTrailingComma_UnexpectedComma()
        {
            // Arrange
            var source = SourceText.From("add(1, )");
            var diagnostics = new DiagnosticBag();

            // Act
            var _ = SyntaxTree.Parse(source, diagnostics);
            var messages = diagnostics.ToList();

            // Assert
            Assert.That(messages.Count(), Is.EqualTo(1));
            Assert.That(messages[0].Kind, Is.EqualTo(DiagnosticKind.SyntaxError));

            var unexpectedComma = messages[0] as SyntaxError;
            //Assert.That(syntaxError.ErrorKind, Is.EqualTo(SyntaxErrorKind.UnexpectedComma));
            Assert.That(unexpectedComma.Location.Span.Start, Is.EqualTo(5));
            Assert.That(unexpectedComma.Location.Span.Length, Is.EqualTo(1));
        }

        [Test]
        public void Parse_WithMissingCloseParentheses_MissingCloseParentheses()
        {
            // Arrange
            var source = SourceText.From("add(1");
            var diagnostics = new DiagnosticBag();

            // Act
            var _ = SyntaxTree.Parse(source, diagnostics);
            var messages = diagnostics.ToList();

            // Assert
            Assert.That(messages.Count(), Is.EqualTo(1));

            Assert.That(messages[0].Kind, Is.EqualTo(DiagnosticKind.SyntaxError));

            var missingParentheses = messages[0] as SyntaxError;
            //Assert.That(syntaxError.ErrorKind, Is.EqualTo(SyntaxErrorKind.MissingCloseParentheses));
            Assert.That(missingParentheses.Location.Span.Start, Is.EqualTo(5));
            Assert.That(missingParentheses.Location.Span.Length, Is.EqualTo(0));
        }
    }
}
