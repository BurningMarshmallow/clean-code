namespace Markdown
{
    public class RendererSettings
    {
        public readonly string Style;
        public readonly string BaseUrl;

        public RendererSettings(string baseUrl=null, string style=null)
        {
            Style = style;
            BaseUrl = baseUrl;
        }
    }
}
