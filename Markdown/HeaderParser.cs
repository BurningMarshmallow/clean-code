using System.Collections.Generic;

namespace Markdown
{
    public class HeaderParser : Parser
    {
        private static readonly List<string> headers = new List<string>
        {
            "######",
            "#####",
            "####",
            "###",
            "##",
            "#"
        };

        public HeaderParser(bool markdownAllowed) : base(markdownAllowed)
        {
        }

        public override Line ParseLine(string text)
        {
            foreach (var header in headers)
            {
                if (!text.StartsWith(header))
                    continue;
                var headerLength = header.Length;
                var parseResult = text.Substring(headerLength);
                return new Line(parseResult, LineType.HeaderLine, $"<h{headerLength}>", $"</h{headerLength}>");
            }
            return new Line(text, LineType.BasicLine, "", "");
        }
    }
}
