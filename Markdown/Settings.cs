namespace Markdown
{
    public class Settings
    {
        public readonly string Style;
        public readonly string BaseUrl;

        public Settings(string baseUrl=null, string style=null)
        {
            Style = style;
            BaseUrl = baseUrl;
        }
    }
}
