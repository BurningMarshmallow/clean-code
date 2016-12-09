﻿using System;
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

        private static readonly List<IParser> parsers = new List<IParser>
        {
            new CodeBlockParser(false),
            new OrderedListParser(false),
            new HeaderParser(true)
        };

        private static readonly List<string> tagNames = tags.Select(tag => tag.TagValue).ToList();

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
            var renderedParagraphs = paragraphs.Select(RenderParagraph);
            return renderedParagraphs.JoinLines();
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
            var tokens = tokenizer.GetTokens(text);
            var renderedTokens = RenderTokens(tokens);
            foreach (var parser in parsers)
            {
                var lineToParse = parser.IsMarkdownAllowed() ? renderedTokens : text;
                var parsedResult = parser.ParseLine(lineToParse);
                if (parsedResult.Type != LineType.BasicLine)
                {
                    return parsedResult;
                }
            }
            return new Line(renderedTokens, LineType.BasicLine, "", "");
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
            return renderedParagraph.JoinLines();
        }

        private string RenderTokens(List<string> tokens)
        {
            var renderedTokens = new Stack<string>();
            var unrenderedTags = new Stack<Tag>();
            var tokensLength = tokens.Count;
            for (var tokenIndex = 0; tokenIndex < tokensLength; tokenIndex++)
            {
                if (IsLink(tokens, tokenIndex))
                {
                    renderedTokens.Push(renderer.RenderLink(tokens, tokenIndex));
                    tokenIndex += 5;
                    continue;
                } 
                var renderedToken = GetRenderedTokenOfTag(tokens, renderedTokens, tokenIndex, unrenderedTags);
                renderedTokens.Push(renderedToken);
            }
            return string.Join("", renderedTokens.Reverse());
        }

        private string GetRenderedTokenOfTag(IReadOnlyList<string> tokens, Stack<string> renderedTokens, int tokenIndex, Stack<Tag> unrenderedTags)
        {
            var token = tokens[tokenIndex];
            var currentTag = tags.FirstOrDefault(tag => tag.TagValue == token);

            if (currentTag == null)
            {
                return token;
            }

            if (IsEscaped(renderedTokens))
            {
                renderedTokens.Pop();
                return token;
            }

            var tagValue = currentTag.TagValue;
            var lastBias = GetLastBias(unrenderedTags, currentTag);

            if (IsIncorrectSurrounding(tokens, lastBias, tokenIndex))
            {
               return tagValue;
            }
            if (lastBias != 0)
                currentTag.Bias = -lastBias;

            var topTag = unrenderedTags.Count > 0 ? unrenderedTags.Peek() : null;
            if (topTag != null && topTag.TagValue == currentTag.TagValue)
            {
                unrenderedTags.Pop();
                var tagBody = GetTagTokensList(renderedTokens, currentTag.TagValue);
                return renderer.RenderTag(tagBody, currentTag);
            }
            unrenderedTags.Push(currentTag);
            return tagValue;
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
