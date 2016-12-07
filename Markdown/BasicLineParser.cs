namespace Markdown
{
    public class BasicLineParser : Parser
    {
        public BasicLineParser(bool markdownAllowed) : base(markdownAllowed)
        {
        }

        public override Line ParseLine(string text)
        {
            return new Line(text, LineType.BasicLine, "", "");
        }
    }
}
