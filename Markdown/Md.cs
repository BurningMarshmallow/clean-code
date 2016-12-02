using System;
using System.Collections.Generic;
using System.Linq;

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
        };

        private static readonly List<Parser> parsers = new List<Parser>
        {
            new CodeBlockParser(false),
            new OrderedListParser(false),
            new HeaderParser(true),
            new BasicLineParser(true)
        };

        private static readonly List<string> tagNames = tags.Select(tag => tag.TagValue).ToList();
        private Stack<Tag> unrenderedTags;

        public Md(HtmlRenderer renderer)
        {
            this.renderer = renderer;
            var escapeAndBrackets = new[] { "\\", "[", "]", "(", ")" };
            var tagNamesAndEscapeAndBrackets = tagNames.Concat(escapeAndBrackets);
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
            var renderedParagraphLines = paragraphLines.Select(renderer.RenderLessOrGreater)
                                                       .Select(GetParsedLine)
                                                       .ToList();
            return $"<p>{JoinRenderedParagraphsLines(renderedParagraphLines)}</p>";
        }

        private Line GetParsedLine(string text)
        {
            foreach (var parser in parsers)
            {
                var lineToParse = text;
                if (parser.MarkdownAllowed)
                {
                    var tokens = tokenizer.GetTokens(text);
                    var renderedTokens = RenderTokens(tokens);
                    lineToParse = Unescape(renderedTokens);
                }
                var parsedResult = parser.ParseLine(lineToParse);
                if (parsedResult != null)
                {
                    return parsedResult;
                }
            }
            return null;
        }

        private static string JoinRenderedParagraphsLines(IReadOnlyList<Line> renderedParagraphLines)
        {
            var numberOfLines = renderedParagraphLines.Count;
            var previousLineType = LineType.None;
            var renderedParagraph = new List<string>();
            for (var lineIndex = 0; lineIndex < numberOfLines; lineIndex++)
            {
                var line = renderedParagraphLines[lineIndex];
                if (line.Type != previousLineType)
                {
                    if (lineIndex > 0)
                        renderedParagraph[lineIndex - 1] += renderedParagraphLines[lineIndex - 1].ClosingTag;
                    renderedParagraph.Add(line.OpeningTag + line.Value);
                }
                else
                {
                    renderedParagraph.Add(line.Value);
                }
                previousLineType = line.Type;
            }
            renderedParagraph[numberOfLines - 1] += renderedParagraphLines[numberOfLines - 1].ClosingTag;
            return JoinLines(renderedParagraph);
        }

        private string RenderTokens(List<string> tokens)
        {
            var renderedTokens = new Stack<string>();
            unrenderedTags = new Stack<Tag>();
            var tokensLength = tokens.Count;
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
            var lastBias = GetLastBias(unrenderedTags, currentTag);

            if (IsIncorrectSurrounding(tokens, lastBias, tokenIndex))
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
            foreach (var tagName in tagNames)
            {
                text = text.Replace("\\" + tagName, tagName);
            }
            return text;
        }
    }
}
