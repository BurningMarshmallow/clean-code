﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class HtmlRenderer
    {
        private readonly string baseUrl;
        private readonly string styleString;

        public HtmlRenderer(Settings settings)
        {
            if (settings.BaseUrl != null)
            {
                baseUrl = settings.BaseUrl;
            }
             styleString = settings.Style == null ? "" : $@" style=""{settings.Style}""";
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
            input[0] = $"<{tagName}{styleString}>";
            input[input.Count - 1] = $"</{tagName}>";
            return string.Join("", input);
        }

        public string RenderLink(List<string> tokens, int tokenIndex)
        {
            // В Markdown ссылка состоит из 6 токенов:
            // [ LinkText ] ( URL )
            // 0     1    2 3  4  5
            var linkText = tokens[tokenIndex + 1];
            var url = tokens[tokenIndex + 4];
            var isAbsoluteUrl = url.StartsWith("http") || url.StartsWith("www");
            return isAbsoluteUrl ? $"<a href=\"{url}\"{styleString}>{linkText}</a>" 
                : $"<a href=\"{baseUrl}{url}\"{styleString}>{linkText}</a>";
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