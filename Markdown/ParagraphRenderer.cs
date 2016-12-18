using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markdown
{
    public class ParagraphRenderer
    {
        private readonly HtmlRenderer renderer;
        private readonly Tokenizer tokenizer;


        public ParagraphRenderer(HtmlRenderer renderer, Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.renderer = renderer;
        }

        private static readonly List<IParser> parsers = new List<IParser>
        {
            new CodeBlockParser(false),
            new OrderedListParser(false),
            new HeaderParser(true)
        };

        public string RenderParagraph(List<string> paragraphLines)
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
            var renderedTokens = renderer.RenderTokens(tokens);
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
            var renderedParagraph = new StringBuilder();
            renderedParagraph.Append(renderedParagraphLines[0].OpeningTag);
            for (var lineIndex = 0; lineIndex < numberOfLines - 1; lineIndex++)
            {
                var line = renderedParagraphLines[lineIndex];
                var nextLine = renderedParagraphLines[lineIndex + 1];

                if (line.Type != nextLine.Type)
                {
                    renderedParagraph.AppendLine(line.Value + line.ClosingTag);
                    renderedParagraph.Append(nextLine.OpeningTag);
                }
                else
                {
                    renderedParagraph.AppendLine(line.Value);
                }
            }
            var lastLine = renderedParagraphLines[numberOfLines - 1];
            renderedParagraph.Append(lastLine.Value + lastLine.ClosingTag);
            return renderedParagraph.ToString();
        }
    }
}
