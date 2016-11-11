using System.Collections.Generic;
using System.Linq;

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

        private string Render(List<string> input, string tagName, bool digitsNotAllowed=true)
        {
            if (digitsNotAllowed && IsOnlyDigitsInBody(input))
                return string.Join("", input);
            input[0] = $"<{tagName}>";
            input[input.Count - 1] = $"</{tagName}>";
            return string.Join("", input);
        }

        public bool IsOnlyDigitsInBody(List<string> input)
        {
            var len = input.Count;
            for (var i = 1; i < len - 1; i++)
            {
                if (!input[i].All(char.IsDigit))
                    return false;
            }
            return true;
        }

        public string RenderUnderscore(List<string> input)
        {
            return Render(input, "em");
        }

        public string RenderDoubleUnderscore(List<string> input)
        {
            return Render(input, "strong");
        }

        public string RenderBacktick(List<string> input)
        {
            return Render(input, "code", false);
        }
    }
}
