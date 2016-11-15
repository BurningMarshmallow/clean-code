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
            new Tag("*", "strong"),
            new Tag("_", "em"),
            new Tag("`", "code", 0, false)
        };

        private static readonly List<string> TagNames = Tags.Select(tag => tag.TagValue).ToList();
        private static readonly List<string> Escapes = new List<string> { "\\" };

        public Md()
        {
            renderer = new HtmlRenderer();
            var tagNamesAndEscape = TagNames.Concat(Escapes);
            tokenizer = new Tokenizer(tagNamesAndEscape.ToArray());
        }

        public string RenderToHtml(string markdown)
        {
            var result = renderer.RenderLessOrGreater(markdown);
            var tokens = tokenizer.GetTokens(result);
            var renderedTokens = RenderTokens(tokens);
            var unescaped = Unescape(renderedTokens);
            return $"<p>{unescaped}</p>";
        }

        private string RenderTokens(IReadOnlyList<Token> tokens)
        {
            var stack = new Stack<string>();
            var tags = new Stack<Tag>();
            var tokenIndex = 0;
            var lastCodeIndex = GetLastCodeIndex(tokens);
            var insideCode = false;
            foreach (var token in tokens)
            {
                tokenIndex++;
                var currentTag = Tags.FirstOrDefault(tag => tag.TagValue == token.TokenValue);

                if (currentTag == null || IsEscaped(stack))
                {
                    stack.Push(token.TokenValue);
                    continue;
                }

                var tagValue = currentTag.TagValue;
                if (tagValue == "`")
                {
                    insideCode = !insideCode && (tokenIndex < lastCodeIndex);
                }
                var lastBias = GetLastBias(tags, currentTag);

                if (IsIncorrectSurrounding(tokens, lastBias, tokenIndex - 1) || DisabledByCodeTag(tagValue, insideCode))
                {
                    stack.Push("\\" + tagValue);
                    continue;
                }
                if (lastBias != 0)
                    currentTag.Bias = -lastBias;

                tags = UpdateTagsWithCurrentTag(tags, currentTag);
                if (!stack.Contains(tagValue))
                {
                    stack.Push(tagValue);
                }
                else
                {
                    stack = AddRenderedTagBody(stack, currentTag);
                }
            }
            return string.Join("", stack.Reverse());
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
            return stack.Count != 0 && Escapes.Contains(stack.Peek());
        }

        private static bool IsIncorrectSurrounding(IReadOnlyList<Token> tokens, int bias, int tokenIndex)
        {
            if (tokenIndex + bias >= tokens.Count || tokenIndex + bias < 0 || bias == 0)
                return false;
            return bias == -1 ? tokens[tokenIndex + bias].TokenValue.EndsWith(" ")
                : tokens[tokenIndex + bias].TokenValue.StartsWith(" ");
        }

        private static int GetLastCodeIndex(IReadOnlyList<Token> tokens)
        {
            var lastCodeIndex = -1;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].TokenValue == "`")
                    lastCodeIndex = i;
            }
            return lastCodeIndex;
        }
        
        private Stack<string> AddRenderedTagBody(Stack<string> stack, Tag tag)
        {
            var tokens = GetTagTokensList(stack, tag.TagValue);
            stack.Push(renderer.Render(tokens, tag, tag.DigitsNotAllowed));
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
            foreach (var tagName in TagNames)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var escapeSymbol in Escapes)
                {
                    text = text.Replace(escapeSymbol + tagName, tagName);
                }
            }
            return text;
        }
    }
}
