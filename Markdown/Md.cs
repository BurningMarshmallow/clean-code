using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
	public class Md
	{
        private readonly HtmlRenderer renderer;

        public Md()
        {
            renderer = new HtmlRenderer();
        }

        public string RenderToHtml(string markdown)
        {
            var converted = ConvertText(markdown);
            return renderer.RenderToParagraph(converted);
        }

        public string ConvertText(string text)
        {
            text = renderer.RenderCompare(text);
            var tokens = GetTokens(text);
            return RemoveSlashes(GetRenderedText(tokens));
        }

        private string GetRenderedText(IReadOnlyList<string> tokens)
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
                        if (IsTokenInsideCode(tokens.ToList(), tokenIndex - 1))
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
            return (stack.Count != 0 && stack.Peek() == "\\")
                || !IsTag(token)
                || (IsTag(token) && !stack.Contains(token));
        }

        private static bool IsTag(string token)
        {
            var tags = new HashSet<string>()
            {
                "_", "__", "`"
            };
            return tags.Contains(token);
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

        public string RemoveSlashes(string text)
        {
            text = text.Replace("\\_", "_")
                       .Replace("\\`", "`")
                       .Replace("\\__", "__");
            return text;
        }

        public List<string> GetTokens(string text)
        {
            return SplitWithDelimiters(text, new[] { "__", "_", "\\", "`" })
                       .Where(s => s != "")
                       .ToList(); ;
        }

	    public List<string> SplitWithDelimiters(string text, string[] delimiters)
	    {
	        var splitted = new List<string>();
            var sb = new StringBuilder();
	        var textLen = text.Length;
	        var i = 0;
            while (i < textLen)
            {
                var isDelim = false;
	            foreach (var delim in delimiters)
	            {
	                if (i + delim.Length > textLen || text.Substring(i, delim.Length) != delim) continue;
	                splitted.Add(sb.ToString());
	                splitted.Add(delim);
	                sb.Clear();
	                isDelim = true;
	                i += delim.Length;
	                break;
	            }
                if (isDelim) continue;
                sb.Append(text[i]);
                i++;
            }
	        if (sb.Length > 0)
	            splitted.Add(sb.ToString());
            return splitted;
	    }
    }
}

