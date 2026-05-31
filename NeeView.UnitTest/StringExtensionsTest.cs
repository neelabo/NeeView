using System;
using NeeLaboratory.Text;

namespace NeeView.UnitTest
{
    public class StringExtensionsTests
    {
        [Fact]
        public void ToOneLine_NoNewline_ReturnsSame()
        {
            var input = "Hello World";
            var actual = input.ToOneLine();
            Assert.Equal("Hello World", actual);
        }

        [Fact]
        public void ToOneLine_CRLf_ReplacedWithSpace()
        {
            var input = "Line1\r\nLine2";
            var actual = input.ToOneLine();
            Assert.Equal("Line1 Line2", actual);
        }

        [Fact]
        public void ToOneLine_LF_ReplacedWithSpace()
        {
            var input = "A\nB";
            var actual = input.ToOneLine();
            Assert.Equal("A B", actual);
        }

        [Fact]
        public void ToOneLine_CR_ReplacedWithSpace()
        {
            var input = "A\rB";
            var actual = input.ToOneLine();
            Assert.Equal("A B", actual);
        }

        [Fact]
        public void ToOneLine_MixedNewlines_AllReplacedPreservingCount()
        {
            var input = "A\rB\nC\r\nD";
            var actual = input.ToOneLine();
            Assert.Equal("A B C D", actual);
        }

        [Fact]
        public void ToOneLine_ConsecutiveNewlines_ProducesMultipleSpaces()
        {
            var input = "A\r\n\r\nB\n\nC\rD";
            var actual = input.ToOneLine();
            // CRLF x2 -> two spaces, \n\n -> two spaces, \r -> one space
            Assert.Equal("A  B  C D", actual);
        }

        [Fact]
        public void ToOneLine_EmptyAndNull_ReturnsEmptyOrNull()
        {
            string empty = string.Empty;
            string? nil = null;

            Assert.Equal(string.Empty, empty.ToOneLine());
            Assert.Null(nil.ToOneLine());
        }
    }
}