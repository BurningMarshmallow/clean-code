using System.Linq;
using System.Text.RegularExpressions;

namespace Markdown
{
    public class HtmlRenderer
    {
        public string RenderToParagraph(string input)
        {
            return $"<p>{input}</p>";
        }

        public string RenderCompare(string input)
        {
            input = input.Replace("\\<", "&lt;")
                         .Replace("\\>", "&gt;");
            return input;
        }

        private string Render(string input, string token, string tag, bool digitsNotAllowed=true)
        {
            if (digitsNotAllowed && IsOnlyDigitsInBody(input, token))
                return input;
            return Regex.Replace(input, $"{token}(.*){token}", $"<{tag}>$1</{tag}>");
        }

        public bool IsOnlyDigitsInBody(string input, string token)
        {
            string regexp = $"{token}(.*){token}";
            var data = Regex.Match(input, regexp);
            if (data.Groups.Count == 2)
                return data.Groups[1].Value.All(char.IsDigit);
            return false;
        }

        public string RenderUnderscore(string input)
        {
            return Render(input, "_", "em");
        }

        public string RenderDoubleUnderscore(string input)
        {
            return Render(input, "__", "strong");
        }

        public string RenderBacktick(string input)
        {
            return Render(input, "`", "code", false);
        }
    }
}
