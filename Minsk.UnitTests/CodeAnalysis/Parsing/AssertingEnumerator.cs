using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Lexing;
using Minsk.CodeAnalysis.Parsing;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Parsing
{
    internal sealed class AssertingEnumerator : IDisposable
    {
        // ToDo: maybe create SyntaxTreeEnumerator?

        private readonly IEnumerator<SyntaxNode> enumerator;
        private bool hasErrors = false;

        public AssertingEnumerator(SyntaxNode node)
        {
            enumerator = Flattern(node).GetEnumerator();
        }

        public void AssertBinaryExpression(TokenKind op)
        {
            try
            {
                Assert.That(enumerator.MoveNext());
                Assert.That(enumerator.Current.Kind, Is.EqualTo(SyntaxKind.BinaryExpression));

                var binaryExpression = enumerator.Current as BinaryExpression;

                Assert.That(binaryExpression.OperatorToken.Kind, Is.EqualTo(op));
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertNameExpression(string identifer)
        {
            try
            {
                Assert.That(enumerator.MoveNext());
                Assert.That(enumerator.Current.Kind, Is.EqualTo(SyntaxKind.NameExpression));

                var nameExpression = enumerator.Current as NameExpression;

                Assert.That(nameExpression.IdentifierToken.Text, Is.EqualTo(identifer));
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertUnaryExpression(TokenKind op)
        {
            try
            {
                Assert.That(enumerator.MoveNext());
                Assert.That(enumerator.Current.Kind, Is.EqualTo(SyntaxKind.UnaryExpression));

                var unaryExpression = enumerator.Current as UnaryExpression;

                Assert.That(unaryExpression.OperatorToken.Kind, Is.EqualTo(op));
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        private static IEnumerable<SyntaxNode> Flattern(SyntaxNode node)
        {
            var stack = new Stack<SyntaxNode>();
            stack.Push(node);

            while(stack.Count > 0)
            {
                var n = stack.Pop();
                yield return n;

                foreach (var child in n.Children.Reverse())
                    stack.Push(child);
            }
        }

        private bool MarkFailed()
        {
            hasErrors = true;
            return false;
        }

        #region IDisposable

        public void Dispose()
        {
            if (!hasErrors)
                Assert.False(enumerator.MoveNext());

            enumerator.Dispose();
        }

        #endregion
    }

    internal static class AssertingEnumeratorExtensions
    {
        public static AssertingEnumerator Enumerate(this SyntaxNode node)
            => new AssertingEnumerator(node);
    }
}
