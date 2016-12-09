using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class HtmlRenderer
    {
        private readonly string baseUrl;
        private readonly string styleString;

        public HtmlRenderer(RendererSettings rendererSettings)
        {
            if (rendererSettings.BaseUrl != null)
            {
                baseUrl = rendererSettings.BaseUrl;
            }
             styleString = rendererSettings.Style == null ? "" : $@" style=""{rendererSettings.Style}""";
        }

        public string RenderLessOrGreater(string input)
        {
            input = input.Replace("<", "&lt;")
                         .Replace(">", "&gt;");
            return input;
        }

        public string RenderTag(string[] input, Tag tag)
        {
            var tagName = tag.TagRepresentation;
            if (IsOnlyDigitsInBody(input))
                return string.Join("", input);
            input[0] = $"<{tagName}{styleString}>";
            input[input.Length - 1] = $"</{tagName}>";
            return string.Join("", input);
        }

        public string RenderLink(List<string> tokens, int tokenIndex)
        {
            // В Markdown ссылка состоит из 6 токенов:
            // [ LinkText ] ( URL )
            // 0     1    2 3  4  5
            var linkText = tokens[tokenIndex + 1];
            var url = tokens[tokenIndex + 4];
            var isNotAbsoluteUrl = !Uri.IsWellFormedUriString(url, UriKind.Absolute);
            if (isNotAbsoluteUrl)
                url = baseUrl + url;
            return $"<a href=\"{url}\"{styleString}>{linkText}</a>";
        }

        private static bool IsOnlyDigitsInBody(IReadOnlyCollection<string> input)
        {
            var len = input.Count;
            return input.Skip(1)
                .Take(len - 2)
                .All(s => s.All(char.IsDigit));
        }
    }
}
