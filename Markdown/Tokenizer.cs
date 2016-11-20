using System.Collections.Generic;
using System.Text;

namespace Markdown
{
    class Tokenizer
    {
        private readonly string[] delimiters;

        public Tokenizer(string[] delimiters)
        {
            this.delimiters = delimiters;
        }

        public List<string> GetTokens(string text)
        {
            var splitted = SplitWithDelimiters(text, delimiters);
            return splitted;
        }

        public List<string> SplitWithDelimiters(string text, string[] delims)
        {
            var splitted = new List<string>();
            var textLen = text.Length;
            var i = 0;
            while (i < textLen)
            {
                var token = ReadNextToken(text, delimiters, i, splitted);
                i += token.Length;
            }
            return splitted;
        }

        public string ReadNextToken(string text, string[] delims, int pos, List<string> splitted)
        {
            var token = new StringBuilder();
            while (pos < text.Length)
            {
                foreach (var delim in delims)
                {
                    if (text.IsNotSubstringStartingFrom(delim, pos))
                        continue;
                    if (token.ToString() != "")
                    {
                        splitted.Add(token.ToString());
                        return token.ToString();
                    }
                    splitted.Add(delim);
                    return delim;
                }
                token.Append(text[pos]);
                pos++;
            }
            splitted.Add(token.ToString());
            return token.ToString();
        }
    }
}
