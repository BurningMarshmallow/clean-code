namespace Markdown
{
    public class HtmlTag
    {
        private readonly string mdValue;
        private readonly string tagName;

        public HtmlTag(string value, string name)
        {
            mdValue = value;
            tagName = name;
        }
    }
}
