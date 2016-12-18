using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class HtmlRenderer
    {
        private readonly string baseUrl;
        private readonly string styleString;

        private static readonly Dictionary<string, Tag> tagTable = Md.TagRepresantations
            .ToDictionary(tag => tag.Key,
                tag => new Tag(tag.Key, tag.Value));

        private Stack<Tag> unrenderedTags;
        private Stack<string> renderedTokens;

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

        public RenderResult RenderTag(string[] input, Tag tag)
        {
            var tagName = tag.TagRepresentation;
            // ReSharper disable once InvertIf
            if (!IsOnlyDigitsInBody(input))
            {
                input[0] = $"<{tagName}{styleString}>";
                input[input.Length - 1] = $"</{tagName}>";
            }
            return new RenderResult(string.Join("", input), 1);
        }

        public RenderResult RenderLink(List<string> tokens, int tokenIndex)
        {
            // В Markdown ссылка состоит из 6 токенов:
            // [ LinkText ] ( URL )
            // 0     1    2 3  4  5
            if (!IsLink(tokens, tokenIndex))
            {
                return new RenderResult("", 0);
            }
            var linkText = tokens[tokenIndex + 1];
            var url = tokens[tokenIndex + 4];
            var isNotAbsoluteUrl = !Uri.IsWellFormedUriString(url, UriKind.Absolute);
            if (isNotAbsoluteUrl)
                url = baseUrl + url;
            return new RenderResult($"<a href=\"{url}\"{styleString}>{linkText}</a>", 6);
        }

        private static bool IsOnlyDigitsInBody(IReadOnlyCollection<string> input)
        {
            var len = input.Count;
            return input.Skip(1)
                .Take(len - 2)
                .All(s => s.All(char.IsDigit));
        }

        public string RenderTokens(List<string> tokens)
        {
            renderedTokens = new Stack<string>();
            unrenderedTags = new Stack<Tag>();
            var tokensLength = tokens.Count;
            for (var tokenIndex = 0; tokenIndex < tokensLength; tokenIndex++)
            {
                var renderFunctions = new List<Func<List<string>, int, RenderResult>>()
                {
                    RenderLink,
                    GetRenderedTokenOfTag
                };
                foreach (var renderFunction in renderFunctions)
                {
                    var renderResult = renderFunction(tokens, tokenIndex);
                    if (renderResult.NumberOfRenderedTokens == 0) continue;
                    renderedTokens.Push(renderResult.Value);
                    tokenIndex += renderResult.NumberOfRenderedTokens - 1;
                    break;
                }
            }
            return string.Join("", renderedTokens.Reverse());
        }

        private RenderResult GetRenderedTokenOfTag(List<string> tokens, int tokenIndex)
        {
            var token = tokens[tokenIndex];

            if (!tagTable.ContainsKey(token))
            {
                return new RenderResult(token, 1);
            }

            var currentTag = tagTable[token];

            if (IsEscaped(renderedTokens))
            {
                renderedTokens.Pop();
                return new RenderResult(token, 1);
            }

            var tagValue = currentTag.TagValue;
            var lastBias = GetLastBias(unrenderedTags, currentTag);

            if (IsIncorrectSurrounding(tokens, lastBias, tokenIndex))
            {
                return new RenderResult(tagValue, 1);
            }
            if (lastBias != 0)
                currentTag.Bias = -lastBias;

            var topTag = unrenderedTags.Count > 0 ? unrenderedTags.Peek() : null;
            if (topTag != null && topTag.TagValue == currentTag.TagValue)
            {
                unrenderedTags.Pop();
                var tagBody = GetTagTokensList(renderedTokens, currentTag.TagValue);
                return RenderTag(tagBody, currentTag);
            }
            unrenderedTags.Push(currentTag);
            return new RenderResult(tagValue, 1);
        }

        private static bool IsLink(IReadOnlyList<string> tokens, int tokenIndex)
        {
            if (tokenIndex + 6 > tokens.Count)
                return false;
            return tokens[tokenIndex] == "["
                   && tokens[tokenIndex + 2] == "]"
                   && tokens[tokenIndex + 3] == "("
                   && tokens[tokenIndex + 5] == ")";
        }

        private static int GetLastBias(Stack<Tag> tags, Tag currentTag)
        {
            if (tags.Count == 0)
                return 1;
            var lastTag = tags.Peek();
            return lastTag.TagRepresentation != currentTag.TagRepresentation ? 0 : lastTag.Bias;
        }

        private static bool IsEscaped(Stack<string> stack)
        {
            return stack.Count != 0 && stack.Peek() == "\\";
        }

        private static bool IsIncorrectSurrounding(IReadOnlyList<string> tokens, int bias, int tokenIndex)
        {
            if (tokenIndex + bias >= tokens.Count || tokenIndex + bias < 0 || bias == 0)
                return false;
            return bias == -1 ? tokens[tokenIndex + bias].EndsWith(" ")
                : tokens[tokenIndex + bias].StartsWith(" ");
        }

        private static string[] GetTagTokensList(Stack<string> renderedTokens, string tagName)
        {
            var tokens = new Stack<string>();
            tokens.Push(tagName);
            while (renderedTokens.Peek() != tagName)
                tokens.Push(renderedTokens.Pop());

            tokens.Push(renderedTokens.Pop());

            return tokens.ToArray();
        }
    }
}
