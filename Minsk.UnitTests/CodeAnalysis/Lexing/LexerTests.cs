using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.Utils;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Lexing
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void Lexer_WithNullDiagnosticsBag_ThrowsArgumentNullException()
        {
            // Arrange
            var text = "";

            // Act
            TestDelegate ctor = () => new Lexer(null, text);

            // Assert
            Assert.That(ctor, Throws.ArgumentNullException);
        }

        [Test]
        public void Lexer_WithNullText_ThrowsArgumentNullException()
        {
            // Arrange
            var diagnostics = new DiagnosticBag();

            // Act
            TestDelegate ctor = () => new Lexer(diagnostics, null);

            // Assert
            Assert.That(ctor, Throws.ArgumentNullException);
        }

        [Test]
        public void Lex_WithSingleToken_LexesToken(
            [ValueSource(nameof(Tokens))] Token token)
        {
            // Act
            var tokens = Lexer.Lex(token.Text)
                .TakeWhile(t => t.Kind != TokenKind.EoF)
                .ToList();

            // Assert
            Assert.That(tokens.Count, Is.EqualTo(1));
            Assert.That(tokens[0].Kind, Is.EqualTo(token.Kind));
        }

        [Test]
        public void Lex_WithTokenPairs_LexesTokens(
            [ValueSource(nameof(TokenPairs))] Pair pair)
        {
            // Act
            var tokens = Lexer.Lex(pair.Text)
                .TakeWhile(t => t.Kind != TokenKind.EoF)
                .ToList();

            // Assert
            Assert.That(tokens.Count, Is.EqualTo(2));
            Assert.That(tokens[0].Kind, Is.EqualTo(pair.Kind1));
            Assert.That(tokens[1].Kind, Is.EqualTo(pair.Kind2));
        }

        [Test]
        public void Lex_WithTokenPairsSeperated_LexesTokens(
            [ValueSource(nameof(TokenPairsWithSeperator))] Trip trip)
        {
            // Act
            var tokens = Lexer.Lex(trip.Text)
                .TakeWhile(t => t.Kind != TokenKind.EoF)
                .ToList();

            // Assert
            Assert.That(tokens.Count, Is.EqualTo(3));
            Assert.That(tokens[0].Kind, Is.EqualTo(trip.Kind1));
            Assert.That(tokens[1].Kind, Is.EqualTo(trip.Kind2));
            Assert.That(tokens[2].Kind, Is.EqualTo(trip.Kind3));
        }

        private static IEnumerable<Token> Tokens
            => new List<Token> {
                (TokenKind.Whitespace,          " "),
                (TokenKind.Whitespace,          "\t"),
                (TokenKind.Whitespace,          "\r"),
                (TokenKind.Whitespace,          "\n"),
                (TokenKind.Whitespace,          "\r\n"),

                (TokenKind.Plus,                "+"),
                (TokenKind.Minus,               "-"),
                (TokenKind.Star,                "*"),
                (TokenKind.ForwardSlash,        "/"),
                (TokenKind.Bang,                "!"),
                (TokenKind.BangEquals,          "!="),
                (TokenKind.AmpersandAmperand,   "&&"),
                (TokenKind.Equals,              "="),
                (TokenKind.EqualsEquals,        "=="),
                (TokenKind.PipePipe,            "||"),
                (TokenKind.OpenParenthesis,     "("),
                (TokenKind.CloseParenthesis,    ")"),

                (TokenKind.TrueKeyword,         "true"),
                (TokenKind.FalseKeyword,        "false"),

                (TokenKind.Number,              "1"),
                (TokenKind.Number,              "123"),
                (TokenKind.Number,              "98765"),

                (TokenKind.Identifier,          "well"),
                (TokenKind.Identifier,          "HELLO"),
                (TokenKind.Identifier,          "tHeRe"),
            };

        private static IEnumerable<Pair> TokenPairs
            => Tokens
                .Where(t => t.Kind != TokenKind.Whitespace)
                .CartesianJoin((p1, p2) => new Pair(p1.Kind, p2.Kind, p1.Text + p2.Text))
                .Where(p => !p.Kind1.RequiresSeperator(p.Kind2));

        private static IEnumerable<Trip> TokenPairsWithSeperator
            => Tokens
                .Where(t => t.Kind != TokenKind.Whitespace)
                .CartesianJoin((p1, p2) => (Kind1: p1.Kind, Text1: p1.Text, Kind2: p2.Kind, Text2: p2.Text))
                .Where(p => p.Kind1.RequiresSeperator(p.Kind2))
                .CrossJoin(
                    Tokens.Where(t => t.Kind == TokenKind.Whitespace),
                    (p, w) => new Trip(p.Kind1, w.Kind, p.Kind2, p.Text1 + w.Text + p.Text2));

        public readonly struct Token
        {
            public Token(TokenKind kind, string text)
            {
                Kind = kind;
                Text = text;
            }

            public TokenKind Kind { get; }
            public string Text { get; }

            public static implicit operator Token((TokenKind, string) tuple)
                => new Token(tuple.Item1, tuple.Item2);
        }

        public readonly struct Pair
        {
            public Pair(TokenKind kind1, TokenKind kind2, string text)
            {
                Kind1 = kind1;
                Kind2 = kind2;
                Text = text;
            }

            public TokenKind Kind1 { get; }
            public TokenKind Kind2 { get; }
            public string Text { get; }

            public static implicit operator Pair((TokenKind, TokenKind, string) tuple)
                => new Pair(tuple.Item1, tuple.Item2, tuple.Item3);
        }

        public readonly struct Trip
        {
            public Trip(TokenKind kind1, TokenKind kind2, TokenKind kind3, string text)
            {
                Kind1 = kind1;
                Kind2 = kind2;
                Kind3 = kind3;
                Text = text;
            }

            public TokenKind Kind1 { get; }
            public TokenKind Kind2 { get; }
            public TokenKind Kind3 { get; }
            public string Text { get; }

            public static implicit operator Trip((TokenKind, TokenKind, TokenKind, string) tuple)
                => new Trip(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        }

        //private void Test(string text, params TokenKind[] expected)
        //{
        //    // Act
        //    var tokens = Lexer.Lex(text).TakeWhile(t => t.Kind != TokenKind.EoF).ToList();

        //    // Assert
        //    Assert.That(tokens.Count, Is.EqualTo(expected.Length));

        //    var tokensIt = tokens.GetEnumerator();
        //    var expectedIt = expected.GetEnumerator();

        //    while (tokensIt.MoveNext() && expectedIt.MoveNext())
        //    {
        //        Assert.That(tokensIt.Current.Kind, Is.EqualTo(expectedIt.Current));
        //    }
        //}
    }
}