﻿using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Diagnostics;
using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;
using Minsk.CodeAnalysis.Text;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Parsing
{
    [TestFixture]
    public class ParserTests
    {
        // ToDo: Add tests that check emission of diagnostics in ParserDiagnosticTests

        [Test]
        public void Constructor_WithNullDiagnosticsBag_ThrowsArgumentNull()
        {
            // Arrange
            var diagnostics = new DiagnosticBag();
            var source = SourceText.From("");
            var syntaxTree = new SyntaxTree(source, diagnostics);

            // Act
            TestDelegate ctor = () => new Parser(syntaxTree, source, null);

            // Assert
            Assert.That(ctor, Throws.ArgumentNullException);
        }

        [Test, Combinatorial]
        public void Parse_WithBinaryExpressions_HonorsPrecedences(
            [ValueSource(nameof(BinaryOperators))] TokenKind op1,
            [ValueSource(nameof(BinaryOperators))] TokenKind op2)
        {
            // Arrange
            var op1Precedence = op1.BinaryOperatorPrecedence();
            var op2Precedence = op2.BinaryOperatorPrecedence();

            var op1Text = op1.GetText();
            var op2Text = op2.GetText();

            var text = $"a {op1.GetText()} b {op2.GetText()} c";

            // Act
            var expression = ParseExpression(text);

            // Assert
            Assert.That(op1Text, Is.Not.Null);
            Assert.That(op2Text, Is.Not.Null);

            // Assert
            if (op1Precedence >= op2Precedence)
            {
                // Expected:
                //      op2
                //     /  \
                //   op1   c
                //  /  \
                // a    b

                using var e = expression.Enumerate();

                e.AssertBinaryExpression(op2);
                e.AssertBinaryExpression(op1);
                e.AssertNameExpression("a");
                e.AssertNameExpression("b");
                e.AssertNameExpression("c");
            }
            else
            {
                // Expected:
                //   op1
                //  /  \
                // a   op2
                //    /  \
                //   b    c

                using var e = expression.Enumerate();

                e.AssertBinaryExpression(op1);
                e.AssertNameExpression("a");
                e.AssertBinaryExpression(op2);
                e.AssertNameExpression("b");
                e.AssertNameExpression("c");
            }
        }

        [Test, Combinatorial]
        public void Parse_WithBinaryUnaryPair_HonorsPrecedences(
            [ValueSource(nameof(UnaryOperators))]  TokenKind unaryOp,
            [ValueSource(nameof(BinaryOperators))] TokenKind binaryOp)
        {
            // Arrange
            var unaryPrecedence = unaryOp.UnaryOperatorPrecedence();
            var binaryPrecedence = binaryOp.BinaryOperatorPrecedence();

            var unaryText = unaryOp.GetText();
            var binaryText = binaryOp.GetText();

            var text = $"{unaryText} a {binaryText} b";

            // Act
            var expression = ParseExpression(text);

            // Assert
            if (unaryPrecedence >= binaryPrecedence)
            {
                // Expected:
                //    binary
                //    /    \
                // unary    b
                //   |
                //   a

                using var e = expression.Enumerate();

                e.AssertBinaryExpression(binaryOp);
                e.AssertUnaryExpression(unaryOp);
                e.AssertNameExpression("a");
                e.AssertNameExpression("b");
            }
            else
            {
                // Expected:
                //  unary
                //    |
                //  binary
                //   /  \
                //  a    b

                using var e = expression.Enumerate();

                e.AssertUnaryExpression(unaryOp);
                e.AssertBinaryExpression(binaryOp);
                e.AssertNameExpression("a");
                e.AssertNameExpression("b");
            }
        }

        [Test, Combinatorial]
        public void Parse_WithUnaryPair_BuildsTree(
            [ValueSource(nameof(UnaryOperators))] TokenKind op1,
            [ValueSource(nameof(UnaryOperators))] TokenKind op2)
        {
            // Arrange
            var op1Text = op1.GetText();
            var op2Text = op2.GetText();

            var text = $"{op1Text} {op2Text} a";

            // Act
            var expression = ParseExpression(text);

            // Assert expected:
            //   op1
            //    |
            //   op2
            //   / \
            //  a   b
            using var e = expression.Enumerate();

            e.AssertUnaryExpression(op1);
            e.AssertUnaryExpression(op2);
            e.AssertNameExpression("a");
        }

        private Expression ParseExpression(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var root = syntaxTree.Root.Members.First();

            Assert.That(root, Is.TypeOf<GlobalStatementSyntax>());

            var expression = ((GlobalStatementSyntax)root).Statement;

            return (expression as ExpressionStatement).Expression;
        }

        private static IEnumerable<TokenKind> TokenKinds
            => new List<TokenKind> {
                TokenKind.Plus,
                TokenKind.Minus,
                TokenKind.Star,
                TokenKind.ForwardSlash
            };

        // ToDo: Move to SyntaxFacts?
        private static IEnumerable<TokenKind> UnaryOperators
            => Enum.GetValues<TokenKind>().Where(k => k.IsUnaryOperator());

        // ToDo: Move to SyntaxFacts?
        private static IEnumerable<TokenKind> BinaryOperators
            => Enum.GetValues<TokenKind>().Where(k => k.IsBinaryOperator());
    }
}
