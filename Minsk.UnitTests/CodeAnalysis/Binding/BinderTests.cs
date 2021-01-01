using System.Collections.Generic;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Parsing;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Binding
{
    [TestFixture]
    public class BinderTests
    {
        public static IEnumerable<string> GoodCode
            => new List<string> {
                "1",
                "{ 1 }",
                "+1",
                "{ +1 }",
                "1 + 2",
                "{ 1 + 2 }",
                "var a = 10",
                "{ var a = 10 }",
                "for a = 1 to 2 a",
                "{ for a = 1 to 2 a }",
            };

        [Test]
        public void Binder_WithGoodCode_BoundTreeHasParentsSet(
            [ValueSource(nameof(GoodCode))] string code)
        {
            // Arrange
            var tree = SyntaxTree.Parse(code);
            var compilation = new Compilation(tree);

            // Apriori
            Assert.That(tree.Diagnostics, Is.Empty);

            // Assert
            using var e = new AssertingEnumerator(compilation.GlobalScope.Statements[0]);

            e.AssertParentNull();

            while (e.HasNext)
                e.AssertParentNotNull();
        }
    }
}
