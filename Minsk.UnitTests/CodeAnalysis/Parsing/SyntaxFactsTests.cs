using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Parsing
{
    [TestFixture]
    public class SyntaxFactsTests
    {
        [Test]
        public void UnaryOperatorPrecedence_HigherThanBinaryOperatorPrecedence(
            [ValueSource(nameof(UnaryOperators))]   TokenKind unaryOp,
            [ValueSource(nameof(BinaryOperators))]  TokenKind binaryOp)
        {
            // Arrange
            var unaryPrecedence = unaryOp.UnaryOperatorPrecedence();
            var binaryPrecedence = binaryOp.BinaryOperatorPrecedence();

            // Act
            var highter = unaryPrecedence > binaryPrecedence;

            // Assert
            Assert.That(highter, Is.True);
        }


        private static IEnumerable<TokenKind> UnaryOperators
            => Enum.GetValues<TokenKind>().Where(k => k.IsUnaryOperator());

        private static IEnumerable<TokenKind> BinaryOperators
            => Enum.GetValues<TokenKind>().Where(k => k.IsBinaryOperator());
    }
}
