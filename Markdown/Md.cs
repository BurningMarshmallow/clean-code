using System;
using System.Collections.Generic;
using System.Linq;

namespace Markdown
{
    public class Md
    {
        private readonly ParagraphRenderer paragraphRenderer;

        public static readonly Dictionary<string, string> TagRepresantations = new Dictionary<string, string>
        {
            {"__", "strong"},
            {"_", "em"}
        };

        private static readonly List<string> tagNames = TagRepresantations.Keys.ToList();

        public Md(HtmlRenderer renderer)
        {
            var escapeAndBrackets = new[] { "\\", "[", "]", "(", ")" };
            var tagNamesAndEscapeAndBrackets = tagNames.Concat(escapeAndBrackets);
            var tokenizer = new Tokenizer(tagNamesAndEscapeAndBrackets.ToArray());
            paragraphRenderer = new ParagraphRenderer(renderer, tokenizer);
        }

        public string RenderToHtml(string markdown)
        {
            var lines = markdown.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var paragraphs = BuildParagraphsFromLines(lines);
            var renderedParagraphs = paragraphs.Select(paragraphRenderer.RenderParagraph);
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
    }
}
