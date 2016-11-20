using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class HtmlRenderer
    {
        private static string _baseUrl;

        public HtmlRenderer()
        {
        }

        public HtmlRenderer(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public string RenderLessOrGreater(string input)
        {
            input = input.Replace("\\<", "&lt;")
                         .Replace("\\>", "&gt;");
            return input;
        }

        public string RenderTag(List<string> input, Tag tag)
        {
            var tagName = tag.TagRepresentation;
            if (tag.DigitsNotAllowed && IsOnlyDigitsInBody(input))
                return string.Join("", input);
            input[0] = $"<{tagName}>";
            input[input.Count - 1] = $"</{tagName}>";
            return string.Join("", input);
        }

        public string RenderLink(List<string> tokens, int tokenIndex)
        {
            var linkText = tokens[tokenIndex + 1];
            var url = tokens[tokenIndex + 4];
            var isAbsoluteUrl = url.StartsWith("http");
            return isAbsoluteUrl ? $"<a href=\"{url}\">{linkText}</a>" : $"<a href=\"{_baseUrl}{url}\">{linkText}</a>";
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
