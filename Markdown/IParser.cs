namespace Markdown
{
    public interface IParser
    {
        Line ParseLine(string text);

        bool IsMarkdownAllowed();
    }
}