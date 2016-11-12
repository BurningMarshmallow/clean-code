using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
    public class Md
    {
        private readonly HtmlRenderer renderer;
        private readonly Tokenizer tokenizer;
        private static readonly List<string> TagNames = new List<string> {"__", "_", "`" };
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

        private string RenderTokens(IReadOnlyList<string> tokens)
        {
            var stack = new Stack<string>();
            var tokenIndex = 0;

            foreach (var token in tokens)
            {
                tokenIndex++;
                if (AddToStack(stack, token))
                {
                    stack.Push(token);
                    continue;
                }
                switch (token)
                {
                    case "_":
                        if (IsTokenInsideCode(tokens, tokenIndex - 1))
                            stack.Push("_");
                        else
                            stack = StackAddRenderedTagBody(stack, "_", renderer.RenderUnderscore);
                        break;
                    case "__":
                        if (IsTokenInsideCode(tokens, tokenIndex - 1))
                            stack.Push("__");
                        else
                            stack = StackAddRenderedTagBody(stack, "__", renderer.RenderDoubleUnderscore);
                        break;
                    case "`":
                        var list = GetTagTokensList(ref stack, token);
                        stack.Push(renderer.RenderBacktick(list));
                        break;
                    default:
                        continue;
                }
            }
            return string.Join("", stack.Reverse());
        }

        private static bool IsTokenInsideCode(IReadOnlyList<string> tokens, int tokenIndex)
        {
            var codeTagsBeforeToken = tokens.Take(tokenIndex)
                                            .Count(t => t == "`");
            var isCodeAfterToken = tokens.Skip(tokenIndex)
                                         .Contains("`");
            return codeTagsBeforeToken % 2 == 1 && isCodeAfterToken;
        }

        private Stack<string> StackAddRenderedTagBody(Stack<string> stack, string tagName, Func<List<string>, string> render)
        {
            var tokens = GetTagTokensList(ref stack, tagName);
            stack.Push(render(tokens));
            return stack;
        }

        private static bool AddToStack(Stack<string> stack, string token)
        {
            return (stack.Count != 0 && Escapes.Contains(stack.Peek()))
                || !IsTag(token)
                || (IsTag(token) && !stack.Contains(token));
        }

        private static bool IsTag(string token)
        {
            return TagNames.Contains(token);
        }

        public List<string> GetTagTokensList(ref Stack<string> stack, string tagName)
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
