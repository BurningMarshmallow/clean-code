namespace Markdown
{
    public class CodeBlockParser : IParser
    {

        public readonly bool MarkdownAllowed;

        public CodeBlockParser(bool markdownAllowed1)
        {
            MarkdownAllowed = markdownAllowed1;
        }

        public Line ParseLine(string text)
        {
            string parseResult;
            if (text.StartsWith("\t"))
            {
                parseResult = text.Substring(1);
                return new Line(parseResult, LineType.CodeBlockLine, "<pre><code>", "</code></pre>");
            }
            if (!text.StartsWith("    ")) return new Line(text, LineType.BasicLine, "", "");
            parseResult = text.Substring(4);
            return new Line(parseResult, LineType.CodeBlockLine, "<pre><code>", "</code></pre>");
        }

        public bool IsMarkdownAllowed()
        {
            return MarkdownAllowed;
        }
    }
}
