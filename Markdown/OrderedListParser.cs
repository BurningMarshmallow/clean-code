using System;
using System.Linq;

namespace Markdown
{
    public class OrderedListParser : IParser
    {
        public readonly bool MarkdownAllowed;

        public OrderedListParser(bool markdownAllowed1)
        {
            MarkdownAllowed = markdownAllowed1;
        }

        public Line ParseLine(string text)
        {
            var periodIndex = text.IndexOf(".", StringComparison.Ordinal);
            if (!IsListValue(text, periodIndex))
                return new Line(text, LineType.BasicLine, "", "");
            var parseResult = GetListValue(text, periodIndex);
            return new Line(parseResult, LineType.OrderedListLine, "<ol>", "</ol>");
        }

        private static string GetListValue(string text, int periodIndex)
        {
            var currentIndex = periodIndex;
            currentIndex++;
            while (text[currentIndex] == ' ')
                currentIndex++;
            return $"<li>{text.Substring(currentIndex)}</li>";
        }

        private static bool IsListValue(string text, int periodIndex)
        {
            if (periodIndex == -1)
                return false;
            var partBeforeDot = text.Substring(0, periodIndex);
            if (partBeforeDot == "")
                return false;
            var periodBeforeSpaces = periodIndex < text.Length - 1 && text[periodIndex + 1] == ' ';
            if (!periodBeforeSpaces)
                return false;
            var partBeforeDotIsNumber = partBeforeDot.All(char.IsDigit);
            return partBeforeDotIsNumber;
        }

        public bool IsMarkdownAllowed()
        {
            return MarkdownAllowed;
        }
    }
}
