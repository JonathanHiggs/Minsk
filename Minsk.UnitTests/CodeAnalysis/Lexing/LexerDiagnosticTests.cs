using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Lexing
{
    [TestFixture]
    public class LexerDiagnosticTests
    {
        [Test]
        public void Lex_WithNullTerminatorChar_AddUnexpectedNullTerminatorDiagnostic()
        {
            // Arrange
            var source = SourceText.From("\0");
            var diagnostics = new DiagnosticBag();

            // Act
            var _ = Lexer.Lex(source, diagnostics).ToList();
            var messages = diagnostics.ToList();

            // Assert
            Assert.That(messages.Count(), Is.EqualTo(1));
            Assert.That(messages[0].Kind, Is.EqualTo(DiagnosticKind.LexError));

            var lexError = messages[0] as LexError;
            Assert.That(lexError.ErrorKind, Is.EqualTo(LexErrorKind.UnexpectedNullTerminator));
            Assert.That(lexError.Span.Start, Is.EqualTo(0));
            Assert.That(lexError.Span.End, Is.EqualTo(1));
        }

        [Test]
        public void Lex_WithUnterminatedString_AddsUnterminatedStringDiagnostic()
        {
            // Arrange
            var source = SourceText.From("\"text");
            var diagnostics = new DiagnosticBag();

            // Act
            var _ = Lexer.Lex(source, diagnostics).ToList();
            var messages = diagnostics.ToList();

            // Assert
            Assert.That(messages.Count(), Is.EqualTo(1));
            Assert.That(messages[0].Kind, Is.EqualTo(DiagnosticKind.LexError));

            var lexError = messages[0] as LexError;
            Assert.That(lexError.ErrorKind, Is.EqualTo(LexErrorKind.UnterminatedString));
            Assert.That(lexError.Span.Start, Is.EqualTo(0));
            Assert.That(lexError.Span.End, Is.EqualTo(1));
        }

        // ToDo: Add tests that check emission of other diagnostics
    }
}
