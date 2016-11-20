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

        public Md()
        {
            renderer = new HtmlRenderer();
            var escapeAndBrackets = new[] {"\\", "[", "]", "(", ")"};
            var tagNamesAndEscapeAndBrackets = TagNames.Concat(escapeAndBrackets);
            tokenizer = new Tokenizer(tagNamesAndEscapeAndBrackets.ToArray());
        }

        public Md(string baseUrl)
        {
            renderer = new HtmlRenderer(baseUrl);
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
            var tags = new Stack<Tag>();
            var tokensLength = tokens.Count;
            var lastCodeIndex = GetLastCodeIndex(tokens);
            var insideCode = false;
            for (var tokenIndex = 0; tokenIndex < tokensLength; tokenIndex++)
            {
                if (IsLink(tokens, tokenIndex))
                {
                    renderedTokens.Push(renderer.RenderLink(tokens, tokenIndex));
                    tokenIndex += 5;
                    continue;
                }
                var token = tokens[tokenIndex];
                var currentTag = Tags.FirstOrDefault(tag => tag.TagValue == token);

                if (currentTag == null || IsEscaped(renderedTokens))
                {
                    renderedTokens.Push(token);
                    continue;
                }

                var tagValue = currentTag.TagValue;
                if (tagValue == "`")
                {
                    insideCode = !insideCode && (tokenIndex < lastCodeIndex);
                }
                var lastBias = GetLastBias(tags, currentTag);

                if (IsIncorrectSurrounding(tokens, lastBias, tokenIndex) || DisabledByCodeTag(tagValue, insideCode))
                {
                    renderedTokens.Push("\\" + tagValue);
                    continue;
                }
                if (lastBias != 0)
                    currentTag.Bias = -lastBias;

                tags = UpdateTagsWithCurrentTag(tags, currentTag);
                if (!renderedTokens.Contains(tagValue))
                {
                    renderedTokens.Push(tagValue);
                }
                else
                {
                    renderedTokens = AddRenderedTagBody(renderedTokens, currentTag);
                }
            }
            return string.Join("", renderedTokens.Reverse());
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

        private static bool DisabledByCodeTag(string tagValue, bool insideCode)
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
        
        private Stack<string> AddRenderedTagBody(Stack<string> stack, Tag tag)
        {
            var tokens = GetTagTokensList(stack, tag.TagValue);
            stack.Push(renderer.RenderTag(tokens, tag));
            return stack;
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
