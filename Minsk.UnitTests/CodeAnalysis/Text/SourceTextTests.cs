using Minsk.CodeAnalysis.Text;

using NUnit.Framework;

namespace Minsk.UnitTests.CodeAnalysis.Text
{
    [TestFixture]
    public class SourceTextTests
    {
        [Test, Sequential]
        public void From_IncludesLastLine(
            [Values(".", ".\r\n", ".\r\n\r\n")] string text,
            [Values(1, 2, 3)] int expectedLines)
        {
            // Act
            var source = SourceText.From(text);

            // Assert
            Assert.That(source.Lines.Length, Is.EqualTo(expectedLines));
        }
    }
}