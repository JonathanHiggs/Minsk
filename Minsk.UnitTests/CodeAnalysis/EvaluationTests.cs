using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Parsing;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis
{
    [TestFixture]
    public class EvaluationTests
    {
        private static IEnumerable<(string Code, object ExpectedResult)> Cases
            => new List<(string, object)> {
                ("1", 1),
                ("+1", 1),
                ("-1", -1),
                ("12 + 14", 26),
                ("12 - 14", -2),
                ("4 * 2", 8),
                ("9 / 3", 3),
                ("(10)", 10),
                ("12 == 3", false),
                ("3 == 3", true),
                ("12 != 3", true),
                ("3 != 3", false),
                ("3 < 3", false),
                ("4 < 6", true),
                ("3 <= 3", true),
                ("3 <= 2", false),
                ("3 > 3", false),
                ("4 > 3", true),
                ("3 >= 3", true),
                ("3 >= 7", false),
                ("false == false", true),
                ("true == false", false),
                ("false != true", true),
                ("true != false", true),
                ("true", true),
                ("false", false),
                ("!true", false),
                ("!false", true),
                ("{ var a = 10 }", 10),
                ("{ var a = 10 a = a + 14 }", 24),
                ("{ var a = 10 \n a = a + 14 }", 24),
                ("{\r\nvar a = 10\r\na = a + 14\r\n}", 24),
                ("{ var x = 3 * 3 }", 9),
                ("{ var x = (4 * 4) }", 16),
                ("{{ var x = 10 } var x = 11 }", 11),
                ("{ var x = 10 if x == 10 x = 11 }", 11),
                ("{ var x = 10 if x == 11 x = 12 }", 10),
                ("{ var x = 9 if x == 9 x = x + 11 else x = x - 1 }", 20),
                ("{ var x = 10 if x == 9 x = x + 11 else x = x - 1 }", 9),
                ("{ var i = 0 while i < 10 i = i + 1 i }", 10),
                ("{ var i = 0 while (i < 10) i = i + 1 i }", 10),
                ("{ var i = 0 while i < 10 { i = i + 1 } i }", 10),
                ("{ var i = 0 while (i < 10) { i = i + 1 } i }", 10),
                //("", ),
            };

        [Test]
        public void Evaluate_WithGoodCode(
            [ValueSource(nameof(Cases))] (string Code, object ExpectedResult) testCase)
        {
            // Arrange
            var tree = SyntaxTree.Parse(testCase.Code);
            var compilation = new Compilation(tree);
            var variables = new Dictionary<VariableSymbol, object>();

            // Apriori
            Assert.That(tree.Diagnostics.Any(), Is.False);

            // Act
            var result = compilation.Evaluate(variables);

            // Assert
            Assert.That(result.Value, Is.EqualTo(testCase.ExpectedResult));
        }

        private static IEnumerable<AnnotatedText> BadCode
            => (new List<string> {
                // Unexpected character
                //"[@]",
                //"{ [@] }",
                // Undefined operator
                "[+]true",
                "true [+] true",
                // Undeclared variable
                "[a] = 10",
                @"{ [a] = 10 }",
                @"
                {
                    [a] = 10
                }",
                // Variable redeclaration
                @"
                {
                    var x = 10
                    var [x] = 10
                }",
                // Assigning to read-only variable
                @"
                {
                    let x = 10
                    [x] = 11
                }",
                // Invalid assignment
                @"
                {
                    var x = 10
                    x [=] true
                }",
            }).Select(AnnotatedText.Parse);


        [Test]
        public void Evaluate_WithBadCode(
            [ValueSource(nameof(BadCode))] AnnotatedText annotatedText)
        {
            // Arrange
            var tree = SyntaxTree.Parse(annotatedText.Text);
            var bound = Binder.BindGlobalScope(null, tree.Root, tree.Diagnostics);
            var diagnostics = bound.Diagnostics.ToList();

            // Assert
            Assert.That(diagnostics.Count, Is.EqualTo(annotatedText.Spans.Length));
        }
    }
}