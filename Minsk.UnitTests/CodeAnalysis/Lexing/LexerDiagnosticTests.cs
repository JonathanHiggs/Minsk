using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;

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
            var text = "\0";
            var diagnostics = new DiagnosticBag();

            // Act
            var _ = Lexer.Lex(text, diagnostics).ToList();
            var messages = diagnostics.ToList();

            // Assert
            Assert.That(messages.Count(), Is.EqualTo(1));
            Assert.That(messages[0].Kind, Is.EqualTo(DiagnosticKind.LexError));

            var lexError = messages[0] as LexError;
            Assert.That(lexError.ErrorKind, Is.EqualTo(LexErrorKind.UnexpectedNullTerminator));
        }
    }
}
