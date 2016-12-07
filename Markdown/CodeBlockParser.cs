namespace Markdown
{
    public class CodeBlockParser : Parser
    {
        public CodeBlockParser(bool markdownAllowed) : base(markdownAllowed)
        {
        }

        public override Line ParseLine(string text)
        {
            string parseResult;
            if (text.StartsWith("\t"))
            {
                parseResult = text.Substring(1);
                return new Line(parseResult, LineType.CodeBlockLine, "<pre><code>", "</code></pre>");
            }
            if (!text.StartsWith("    ")) return null;
            parseResult = text.Substring(4);
            return new Line(parseResult, LineType.CodeBlockLine, "<pre><code>", "</code></pre>");
        }
    }
}
