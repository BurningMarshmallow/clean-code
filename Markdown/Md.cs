using System;
using System.Collections.Generic;
using System.Linq;

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
            var lastCodeIndex = GetLastCodeIndex(tokens);
            var insideCode = false;
            var biasStrong = 1;
            var biasEm = 1;
            foreach (var token in tokens)
            {
                tokenIndex++;
                if (token == "`")
                {
                    insideCode = !insideCode && (tokenIndex < lastCodeIndex);
                }
                if (IsEscapedOrNotTag(stack, token))
                {
                    stack.Push(token);
                    continue;
                }
                if (IsTag(token) && !stack.Contains(token))
                {
                    var bias = token == "_" ? biasEm : token == "__" ? biasStrong : -1;
                    if (IsIncorrectSurrounding(tokens, bias, tokenIndex - 1))
                    {
                        stack.Push("\\" + token);
                        continue;
                    }
                    if (token == "_")
                    {
                        biasEm = -biasEm;
                    }

                    if (token == "__")
                    {
                        biasStrong = -biasStrong;                        
                    }

                    stack.Push(token);
                    continue;
                }
                switch (token)
                {
                    case "_":
                        if (insideCode || IsIncorrectSurrounding(tokens, biasEm, tokenIndex - 1))
                            stack.Push("_");
                        else
                            stack = StackAddRenderedTagBody(stack, "_", renderer.RenderUnderscore);
                        break;
                    case "__":
                        if (insideCode || IsIncorrectSurrounding(tokens, biasStrong, tokenIndex - 1))
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

        private static bool IsIncorrectSurrounding(IReadOnlyList<string> tokens, int bias, int tokenIndex)
        {
            if (tokenIndex + bias >= tokens.Count || tokenIndex + bias < 0)
                return false;
            return bias == -1 ? tokens[tokenIndex + bias].EndsWith(" ") : tokens[tokenIndex + bias].StartsWith(" ");
        }

        private static int GetLastCodeIndex(IReadOnlyList<string> tokens)
        {
            var lastCodeIndex = -1;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == "`")
                    lastCodeIndex = i;
            }
            return lastCodeIndex;
        }

        private Stack<string> StackAddRenderedTagBody(Stack<string> stack, string tagName, Func<List<string>, string> render)
        {
            var tokens = GetTagTokensList(ref stack, tagName);
            stack.Push(render(tokens));
            return stack;
        }

        private static bool IsEscapedOrNotTag(Stack<string> stack, string token)
        {
            return stack.Count != 0 && Escapes.Contains(stack.Peek())
                   || !IsTag(token) ;
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
