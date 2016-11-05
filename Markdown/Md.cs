using NUnit.Framework;

namespace Markdown
{
	public class Md
	{
		public string RenderToHtml(string markdown)
		{
			return $"<p>{markdown}</p>"; ;
		}

        public static string ProcessTag(string line, HtmlTag tag);

	    private static ParserResult ParseOnFields(string line, HtmlTag tag);

        private static string InsertTags(ParserResult result, HtmlTag tag);

    }

    [TestFixture]
	public class Md_ShouldRender
	{
	    private Md mdProcessor;

        [TestCase("iamatest.STUB", ExpectedResult = "<p>iamatest.STUB</p>")]
        [TestCase("bla-bla-bla", ExpectedResult = "<p>bla-bla-bla</p>")]
        public string ShouldParseSimpleText_AsParagraph(string text)
	    {
	        mdProcessor = new Md();

	        var rendered = mdProcessor.RenderToHtml(text);

	        return rendered;
	    }
    }
}