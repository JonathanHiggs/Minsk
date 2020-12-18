using System.Collections.Generic;
using System.Linq;

using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Common;
using Minsk.CodeAnalysis.Parsing;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis
{
    [TestFixture]
    public class EvaluationTests
    {
        // ToDo: Add tests for errors, eg:
        // "{ let x = 10 x = 11 }" throw assignment of read-only variable

        [Test] 
        public void Evaluate_WithNumericalLiteral(
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
                //("", ),
            };
    }
}