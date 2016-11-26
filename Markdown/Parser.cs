namespace Markdown
{
    public abstract class Parser
    {
        public readonly bool MarkdownAllowed;

        protected Parser(bool markdownAllowed)
        {
            MarkdownAllowed = markdownAllowed;
        }

        public abstract Line ParseLine(string text);
    }
}
