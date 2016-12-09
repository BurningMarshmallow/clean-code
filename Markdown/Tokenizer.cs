using System.Collections.Generic;
using System.Text;

namespace Markdown
{
    public class Tokenizer
    {
        private readonly string[] tokenizerDelimiters;

        public Tokenizer(string[] tokenizerDelimiters)
        {
            this.tokenizerDelimiters = tokenizerDelimiters;
        }

        public List<string> GetTokens(string text)
        {
            var splitted = SplitWithDelimiters(text);
            return splitted;
        }

        private List<string> SplitWithDelimiters(string text)
        {
            var splitted = new List<string>();
            var textLen = text.Length;
            var i = 0;
            while (i < textLen)
            {
                var tokenLength = GetNextTokenLength(text, i, splitted);
                i += tokenLength;
            }
            return splitted;
        }

        private int GetNextTokenLength(string text, int startPosition, List<string> splitted)
        {
            var token = new StringBuilder();
            for (var currentPosition = startPosition; currentPosition < text.Length; currentPosition++)
            {
                foreach (var delim in tokenizerDelimiters)
                {
                    if (text.IsNotSubstringStartingFrom(delim, currentPosition))
                        continue;
                    if (token.Length > 0)
                    {
                        var tokenValue = token.ToString();
                        splitted.Add(tokenValue);
                        return tokenValue.Length;
                    }
                    splitted.Add(delim);
                    return delim.Length;
                }
                token.Append(text[currentPosition]);
            }
            splitted.Add(token.ToString());
            return token.Length;
        }
    }
}
