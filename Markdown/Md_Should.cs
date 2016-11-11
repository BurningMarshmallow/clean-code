using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class Md_Should
    {
        private readonly Md mdProcessor = new Md();

        [TestCase("iamatest.STUB", ExpectedResult = "<p>iamatest.STUB</p>")]
        [TestCase("bla-bla-bla", ExpectedResult = "<p>bla-bla-bla</p>")]
        public string parseSimpleText_AsParagraph(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("first_info_second", ExpectedResult = "<p>first<em>info</em>second</p>")]
        [TestCase("_WWW_", ExpectedResult = "<p><em>WWW</em></p>")]
        public string parseSingleUnderscores_toEmTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("why__ooooo__why", ExpectedResult = "<p>why<strong>ooooo</strong>why</p>")]
        [TestCase("__ABBA__", ExpectedResult = "<p><strong>ABBA</strong></p>")]
        public string parseDoubleUnderscores_toStrongTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("`CODE`", ExpectedResult = "<p><code>CODE</code></p>")]
        public string parseSingleQuotes_toCodeTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase(@"\_shiEld\_", ExpectedResult = "<p>_shiEld_</p>")]
        [TestCase(@"\_\_rapapa\_\_", ExpectedResult = "<p>__rapapa__</p>")]
        [TestCase(@"\`apapa\`", ExpectedResult = "<p>`apapa`</p>")]
        public string parseEscapedTags_asSimpleText(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("abc__def__ghij_KEK_lmnopq", ExpectedResult = "<p>abc<strong>def</strong>ghij<em>KEK</em>lmnopq</p>")]
        [TestCase("__xx__and_yy_", ExpectedResult = "<p><strong>xx</strong>and<em>yy</em></p>")]
        public string parseMultipleConsequentTags_asExpected(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("__text_", ExpectedResult = "<p>__text_</p>")]
        [TestCase("_anothertext__", ExpectedResult = "<p>_anothertext__</p>")]
        public string parseUnpairedTags_asSimpleText(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }
        [TestCase("`_inactive_`", ExpectedResult = "<p><code>_inactive_</code></p>")]
        [TestCase("`it is_passive_word`", ExpectedResult = "<p><code>it is_passive_word</code></p>")]
        public string parseSingleUnderscoresInsideCode_asJustCode(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("some_text_`", ExpectedResult = "<p>some<em>text</em>`</p>")]
        [TestCase("`before_inside_", ExpectedResult = "<p>`before<em>inside</em></p>")]
        public string parseSingleUnderscoresInsideUnpairedCode_asStrong(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }


        [TestCase("`I__am__ inside code`", ExpectedResult = "<p><code>I__am__ inside code</code></p>")]
        [TestCase("`__inside__`", ExpectedResult = "<p><code>__inside__</code></p>")]
        public string parseDoubleUnderscoresInsideCode_asJustCode(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("__test__`abc", ExpectedResult = "<p><strong>test</strong>`abc</p>")]
        [TestCase("`__inside__", ExpectedResult = "<p>`<strong>inside</strong></p>")]
        public string parseDoubleUnderscoresInsideUnpairedCode_asStrong(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }
    }
}
