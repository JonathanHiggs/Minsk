using System;
using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis.Binding;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Binding
{
    internal sealed class AssertingEnumerator : IDisposable
    {
        private readonly Stack<BoundNode> nodeStack;

        private bool hasErrors = false;


        public AssertingEnumerator(BoundNode node)
        {
            nodeStack = new Stack<BoundNode>();
            nodeStack.Push(node);
        }

        public BoundNode Current { get; private set; }

        public bool HasNext => nodeStack.Any();

        public void AssertParentNull()
        {
            try
            {
                Assert.That(MoveNext());
                Assert.That(Current.Parent, Is.Null);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertParentNotNull()
        {
            try
            {
                Assert.That(MoveNext());
                Assert.That(Current.Parent, Is.Not.Null);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void SkipNode()
        {
            try
            {
                Assert.That(MoveNext());
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        private bool MoveNext()
        {
            if (!nodeStack.Any())
                return false;

            Current = nodeStack.Pop();

            foreach (var child in Current.Children.Reverse())
                nodeStack.Push(child);

            return true;
        }

        private bool MarkFailed()
        {
            hasErrors = true;
            return false;
        }

        public void Dispose()
        {
            if (!hasErrors)
                Assert.That(MoveNext(), Is.False);
        }
    }
}
