using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Markdown
{
    public class Md
    {
        private readonly HtmlRenderer renderer;
        private readonly Tokenizer tokenizer;
        private static readonly List<Tag> tags = new List<Tag>
        {
            new Tag("__", "strong"),
            new Tag("_", "em"),
            new Tag("`", "code", 0, false)
        };

        private static readonly List<string> headers = new List<string>
        {
            "######",
            "#####",
            "####",
            "###",
            "##",
            "#"
        };

        private static readonly List<string> TagNames = tags.Select(tag => tag.TagValue).ToList();
        private bool insideCode;
        private int lastCodeIndex;
        private Stack<Tag> unrenderedTags;

        public Md(Settings settings)
        {
            renderer = new HtmlRenderer(settings);
            var escapeAndBrackets = new[] { "\\", "[", "]", "(", ")" };
            var tagNamesAndEscapeAndBrackets = TagNames.Concat(escapeAndBrackets);
            tokenizer = new Tokenizer(tagNamesAndEscapeAndBrackets.ToArray());
        }

        public string RenderToHtml(string markdown)
        {
            var lines = markdown.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var paragraphs = BuildParagraphsFromLines(lines);
            var renderedParagraphs = paragraphs.Select(RenderParagraph).ToList();
            return JoinLines(renderedParagraphs);
        }

        private static IEnumerable<List<string>> BuildParagraphsFromLines(IEnumerable<string> lines)
        {
            var paragraph = new List<string>();
            foreach (var line in lines)
            {
                if (line == "")
                {
                    if (paragraph.Count > 0)
                    {
                        yield return paragraph;
                        paragraph.Clear();
                    }
                    yield return new List<string>();
                }
                else
                {
                    paragraph.Add(line);
                }
            }
            if (paragraph.Count > 0)
            {
                yield return paragraph;
            }
        }

        private static string JoinLines(IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private string RenderParagraph(List<string> paragraphLines)
        {
            if (paragraphLines.Count == 0)
                return "";
            var renderedParagraphLines = new List<Line>();
            foreach (var line in paragraphLines)
            {
                var result = renderer.RenderLessOrGreater(line);
                string parseResult;
                if (TryParseCodeBlock(result, out parseResult))
                {
                    renderedParagraphLines.Add(new Line(parseResult, LineType.CodeBlockLine, "<pre><code>", "</code></pre>"));
                    continue;
                }
                if (TryParseOrderedList(result, out parseResult))
                {
                    renderedParagraphLines.Add(new Line(parseResult, LineType.OrderedListLine, "<ol>", "</ol>"));
                    continue;
                }
                var tokens = tokenizer.GetTokens(result);
                var renderedTokens = RenderTokens(tokens);
                var unescaped = Unescape(renderedTokens);
                if (TryParseHeader(unescaped, out parseResult))
                {
                    var headerLength = GetHeaderLength(unescaped);
                    renderedParagraphLines.Add(new Line(parseResult, LineType.HeaderLine, $"<h{headerLength}>", $"</h{headerLength}>"));
                    continue;
                }
                renderedParagraphLines.Add(new Line(unescaped, LineType.BasicLine, "", ""));
            }
            return $"<p>{JoinRenderedParagraphsLines(renderedParagraphLines)}</p>";
        }

        private static bool TryParseOrderedList(string text, out string parseResult)
        {
            parseResult = null;
            if (!IsValueOfList(text)) return false;
            parseResult = GetListValue(text);
            return true;
        }

        private static string GetListValue(string text)
        {
            var currentIndex = text.IndexOf(".", StringComparison.Ordinal);
            currentIndex++;
            while (text[currentIndex] == ' ')
                currentIndex++;
            return $"<li>{text.Substring(currentIndex)}</li>";
        }

        public static bool IsValueOfList(string text)
        {
            var periodIndex = text.IndexOf(".", StringComparison.Ordinal);
            if (periodIndex == -1)
                return false;
            var partBeforeDot = text.Substring(0, periodIndex);
            if (partBeforeDot == "")
                return false;
            var partBeforeDotIsNumber = partBeforeDot.All(char.IsDigit);
            var periodBeforeSpaces = periodIndex < text.Length - 1 && text[periodIndex + 1] == ' ';
            return partBeforeDotIsNumber && periodBeforeSpaces;
        }

        private static int GetHeaderLength(string text)
        {
            return headers.Where(text.StartsWith).Select(header => header.Length).FirstOrDefault();
        }

        private static string JoinRenderedParagraphsLines(IReadOnlyList<Line> renderedParagraphLines)
        {
            var numberOfLines = renderedParagraphLines.Count;
            var previousLineType = LineType.None;
            var renderedParagraph = new List<string>();
            for (var lineIndex = 0; lineIndex < numberOfLines; lineIndex++)
            {
                var line = renderedParagraphLines[lineIndex];
                if (line.LineType != previousLineType)
                {
                    if (lineIndex > 0)
                        renderedParagraph[lineIndex - 1] += renderedParagraphLines[lineIndex - 1].LineTypeClosingTag;
                    renderedParagraph.Add(line.LineTypeOpeningTag + line.LineValue);
                }
                else
                {
                    renderedParagraph.Add(line.LineValue);
                }
                previousLineType = line.LineType;
            }
            renderedParagraph[numberOfLines - 1] += renderedParagraphLines[numberOfLines - 1].LineTypeClosingTag;
            return JoinLines(renderedParagraph);
        }

        public bool TryParseCodeBlock(string text, out string parseResult)
        {
            parseResult = null;
            if (text.StartsWith("\t"))
            {
                parseResult = text.Substring(1);
                return true;
            }
            if (!text.StartsWith("    ")) return false;
            parseResult = text.Substring(4);
            return true;
        }

        private static bool TryParseHeader(string text, out string parseResult)
        {
            parseResult = null;
            foreach (var header in headers)
            {
                if (!text.StartsWith(header))
                    continue;
                var headerLen = header.Length;
                parseResult = text.Substring(headerLen);
                return true;
            }
            return false;
        }

        private string RenderTokens(List<string> tokens)
        {
            var renderedTokens = new Stack<string>();
            unrenderedTags = new Stack<Tag>();
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
            var currentTag = tags.FirstOrDefault(tag => tag.TagValue == token);

            if (currentTag == null || IsEscaped(renderedTokens))
            {
                return token;
            }

            var tagValue = currentTag.TagValue;
            if (tagValue == "`")
            {
                insideCode = !insideCode && (tokenIndex < lastCodeIndex);
            }
            var lastBias = GetLastBias(unrenderedTags, currentTag);

            if (IsIncorrectSurrounding(tokens, lastBias, tokenIndex) || DisabledByCodeTag(tagValue))
            {
               return "\\" + tagValue;
            }
            if (lastBias != 0)
                currentTag.Bias = -lastBias;

            unrenderedTags = UpdateTagsWithCurrentTag(unrenderedTags, currentTag);
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

        private static List<string> GetTagTokensList(Stack<string> stack, string tagName)
        {
            var tokens = new List<string> { tagName };
            while (stack.Peek() != tagName)
                tokens.Add(stack.Pop());

            tokens.Add(stack.Pop());
            tokens.Reverse();

            return tokens;
        }

        private static string Unescape(string text)
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
