namespace Markdown
{
    class Token
    {
        public string TokenName;
        public string TokenValue;

        public Token(string tokenName, string tokenValue)
        {
            TokenName = tokenName;
            TokenValue = tokenValue;
        }
    }
}
