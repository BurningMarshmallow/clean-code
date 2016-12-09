namespace Markdown
{
    public class HeaderParser : IParser
    {
        private static readonly string[] headers = {
            "######",
            "#####",
            "####",
            "###",
            "##",
            "#"
        };

        public readonly bool MarkdownAllowed;

        public HeaderParser(bool markdownAllowed)
        {
            MarkdownAllowed = markdownAllowed;
        }

        public Line ParseLine(string text)
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

        public bool IsMarkdownAllowed()
        {
            return MarkdownAllowed;
        }
    }
}
