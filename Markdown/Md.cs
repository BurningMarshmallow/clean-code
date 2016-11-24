using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class Md
    {
        private readonly HtmlRenderer renderer;
        private readonly Tokenizer tokenizer;
        private static readonly List<Tag> Tags = new List<Tag>
        {
            new Tag("__", "strong"),
            new Tag("_", "em"),
            new Tag("`", "code", 0, false)
        };

        private static readonly List<string> TagNames = Tags.Select(tag => tag.TagValue).ToList();
        private bool insideCode;
        private int lastCodeIndex;
        private Stack<Tag> tags;

        public Md(string baseUrl = "", string style = null)
        {
            renderer = new HtmlRenderer(baseUrl, style);
            var escapeAndBrackets = new[] { "\\", "[", "]", "(", ")" };
            var tagNamesAndEscapeAndBrackets = TagNames.Concat(escapeAndBrackets);
            tokenizer = new Tokenizer(tagNamesAndEscapeAndBrackets.ToArray());
        }

        public string RenderToHtml(string markdown)
        {
            var result = renderer.RenderLessOrGreater(markdown);
            var tokens = tokenizer.GetTokens(result);
            var renderedTokens = RenderTokens(tokens);
            var unescaped = Unescape(renderedTokens);
            return $"<p>{unescaped}</p>";
        }

        private string RenderTokens(List<string> tokens)
        {
            var renderedTokens = new Stack<string>();
            tags = new Stack<Tag>();
            var tokensLength = tokens.Count;
            lastCodeIndex = GetLastCodeIndex(tokens);
            for (var tokenIndex = 0; tokenIndex < tokensLength; tokenIndex++)
            {
                if (IsLink(tokens, tokenIndex))
                {
                    renderedTokens.Push(renderer.RenderLink(tokens, tokenIndex));
                    tokenIndex += 5;
                    continue;
                } 
                var renderedToken = GetRenderedTokenOfTag(tokens, renderedTokens, tokenIndex);
                renderedTokens.Push(renderedToken);
            }
            insideCode = false;
            lastCodeIndex = -1;
            return string.Join("", renderedTokens.Reverse());
        }

        private string GetRenderedTokenOfTag(IReadOnlyList<string> tokens, Stack<string> renderedTokens, int tokenIndex)
        {
            var token = tokens[tokenIndex];
            var currentTag = Tags.FirstOrDefault(tag => tag.TagValue == token);

            if (currentTag == null || IsEscaped(renderedTokens))
            {
                return token;
            }

            var tagValue = currentTag.TagValue;
            if (tagValue == "`")
            {
                insideCode = !insideCode && (tokenIndex < lastCodeIndex);
            }
            var lastBias = GetLastBias(tags, currentTag);

            if (IsIncorrectSurrounding(tokens, lastBias, tokenIndex) || DisabledByCodeTag(tagValue))
            {
               return "\\" + tagValue;
            }
            if (lastBias != 0)
                currentTag.Bias = -lastBias;

            tags = UpdateTagsWithCurrentTag(tags, currentTag);
            if (!renderedTokens.Contains(tagValue))
            {
                return tagValue;
            }
            var tagBody = GetTagTokensList(renderedTokens, currentTag.TagValue);
            return renderer.RenderTag(tagBody, currentTag);
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

        private static Stack<Tag> UpdateTagsWithCurrentTag(Stack<Tag> tags, Tag currentTag)
        {
            if (tags.Count != 0 && tags.Peek().TagValue == currentTag.TagValue)
                tags.Pop();

            tags.Push(currentTag);
            return tags;
        }

        private static int GetLastBias(Stack<Tag> tags, Tag currentTag)
        {
            if (tags.Count == 0)
                return 1;
            var lastTag = tags.Peek();
            return lastTag.TagValue != currentTag.TagValue ? 0 : lastTag.Bias;
        }

        private bool DisabledByCodeTag(string tagValue)
        {
            return tagValue != "`" && insideCode;
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

        private static int GetLastCodeIndex(IReadOnlyList<string> tokens)
        {
            var lastCodeIndex = -1;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i]== "`")
                    lastCodeIndex = i;
            }
            return lastCodeIndex;
        }

        public List<string> GetTagTokensList(Stack<string> stack, string tagName)
        {
            var tokens = new List<string> { tagName };
            while (stack.Peek() != tagName)
                tokens.Add(stack.Pop());

            tokens.Add(stack.Pop());
            tokens.Reverse();

            return tokens;
        }

        public string Unescape(string text)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var tagName in TagNames)
            {
                text = text.Replace("\\" + tagName, tagName);
            }
            return text;
        }
    }
}
