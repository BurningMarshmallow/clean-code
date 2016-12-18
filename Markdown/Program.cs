using System;
using System.IO;

namespace Markdown
{
	class Program
	{
	    private static void Main(string[] args)
		{
            var inputFilename = args[0];
            var outputFilename = args[1];
            var content = File.ReadAllText(inputFilename);
            var rendererSettings = new RendererSettings("", "color:green;");
            var renderer = new HtmlRenderer(rendererSettings);
            var mdProcessor = new Md(renderer);
            var renderedContent = mdProcessor.RenderToHtml(content);
            File.WriteAllText(outputFilename, CreateHtmlPage(renderedContent));
        }

        public static string CreateHtmlPage(string content)
        {
            return "<!DOCTYPE html>\n" +
                   "<html>\n" +
                   "<head>\n" +
                   "    <meta charset='utf-8'>\n" +
                   "</head>\n" +
                   "<body>\n" +
                   "<p>\n    " +
                   content +
                   "\n</p>\n" +
                   "</body>\n" +
                   "</html>";
        }
    }
}
