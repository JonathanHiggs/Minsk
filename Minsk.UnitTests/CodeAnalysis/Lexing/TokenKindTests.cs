using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;

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
            var text = kind.GetText();

            // Act
            var tokens = Lexer.Lex(text).ToList();

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