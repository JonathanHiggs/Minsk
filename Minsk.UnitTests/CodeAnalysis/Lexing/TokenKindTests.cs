using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Text;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Lexing
{
    [TestFixture]
    public class LexerKindTests
    {
        [Test]
        public void GetText_WhenLexed_ReturnsSameKind(
            [ValueSource(nameof(DefaultTextTokens))] TokenKind kind)
        {
            // Arrange
            var source = SourceText.From(kind.GetText());

            // Act
            var tokens = Lexer.Lex(source).ToList();

            // Assert
            Assert.That(tokens.Count, Is.EqualTo(2));
            Assert.That(tokens[0].Kind, Is.EqualTo(kind));
            Assert.That(tokens[1].Kind, Is.EqualTo(TokenKind.EoF));
        }


        private static IEnumerable<TokenKind> DefaultTextTokens
            => Enum.GetValues<TokenKind>()
                .Where(k => k.HasDefaultText());
    }
}