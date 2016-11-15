using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class HtmlRenderer
    {
        public string RenderLessOrGreater(string input)
        {
            input = input.Replace("\\<", "&lt;")
                         .Replace("\\>", "&gt;");
            return input;
        }

        public string Render(List<string> input, Tag tag, bool digitsNotAllowed=true)
        {
            var tagName = tag.TagRepresentation;
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
    }
}
