using System;
using System.Linq;

namespace Markdown
{
    public class OrderedListParser : Parser
    {
        public OrderedListParser(bool markdownAllowed) : base(markdownAllowed)
        {
        }

        public override Line ParseLine(string text)
        {
            if (!IsListValue(text))
                return new Line(text, LineType.BasicLine, "", "");
            var parseResult = GetListValue(text);
            return new Line(parseResult, LineType.OrderedListLine, "<ol>", "</ol>");
        }

        private static string GetListValue(string text)
        {
            var currentIndex = text.IndexOf(".", StringComparison.Ordinal);
            currentIndex++;
            while (text[currentIndex] == ' ')
                currentIndex++;
            return $"<li>{text.Substring(currentIndex)}</li>";
        }

        private static bool IsListValue(string text)
        {
            var periodIndex = text.IndexOf(".", StringComparison.Ordinal);
            if (periodIndex == -1)
                return false;
            var partBeforeDot = text.Substring(0, periodIndex);
            if (partBeforeDot == "")
                return false;
            var partBeforeDotIsNumber = partBeforeDot.All(char.IsDigit);
            var periodBeforeSpaces = periodIndex < text.Length - 1 && text[periodIndex + 1] == ' ';
            return partBeforeDotIsNumber && periodBeforeSpaces;
        }
    }
}
