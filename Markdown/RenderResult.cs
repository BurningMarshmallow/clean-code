namespace Markdown
{
    public struct RenderResult
    {
        public readonly string Value;
        public readonly int NumberOfRenderedTokens;


        public RenderResult(string value, int numberOfRenderedTokens)
        {
            Value = value;
            NumberOfRenderedTokens = numberOfRenderedTokens;
        }
    }
}
