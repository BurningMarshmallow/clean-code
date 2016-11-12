using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class Md_Should
    {
        private readonly Md mdProcessor = new Md();

        [TestCase("iamatest.STUB", ExpectedResult = "<p>iamatest.STUB</p>")]
        [TestCase("bla-bla-bla", ExpectedResult = "<p>bla-bla-bla</p>")]
        public string ParseSimpleText_AsParagraph(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("first_info_second", ExpectedResult = "<p>first<em>info</em>second</p>")]
        [TestCase("_WWW_", ExpectedResult = "<p><em>WWW</em></p>")]
        public string ParseSingleUnderscores_ToEmTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("why__ooooo__why", ExpectedResult = "<p>why<strong>ooooo</strong>why</p>")]
        [TestCase("__ABBA__", ExpectedResult = "<p><strong>ABBA</strong></p>")]
        public string ParseDoubleUnderscores_ToStrongTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("`CODE`", ExpectedResult = "<p><code>CODE</code></p>")]
        public string ParseSingleQuotes_ToCodeTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase(@"\_shiEld\_", ExpectedResult = "<p>_shiEld_</p>")]
        [TestCase(@"\_\_rapapa\_\_", ExpectedResult = "<p>__rapapa__</p>")]
        [TestCase(@"\`apapa\`", ExpectedResult = "<p>`apapa`</p>")]
        public string ParseEscapedTags_AsSimpleText(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("abc__def__ghij_KEK_lmnopq", ExpectedResult = "<p>abc<strong>def</strong>ghij<em>KEK</em>lmnopq</p>")]
        [TestCase("__xx__and_yy_", ExpectedResult = "<p><strong>xx</strong>and<em>yy</em></p>")]
        public string ParseMultipleConsequentTags_OneByOne(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("__text_", ExpectedResult = "<p>__text_</p>")]
        [TestCase("_anothertext__", ExpectedResult = "<p>_anothertext__</p>")]
        public string ParseUnpairedTags_AsSimpleText(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }
        [TestCase("`_inactive_`", ExpectedResult = "<p><code>_inactive_</code></p>")]
        [TestCase("`it is_passive_word`", ExpectedResult = "<p><code>it is_passive_word</code></p>")]
        public string ParseSingleUnderscoresInsideCode_AsJustCode(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("some_text_`", ExpectedResult = "<p>some<em>text</em>`</p>")]
        [TestCase("`before_inside_", ExpectedResult = "<p>`before<em>inside</em></p>")]
        public string ParseSingleUnderscoresInsideUnpairedCode_AsStrong(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }


        [TestCase("`I__am__ inside code`", ExpectedResult = "<p><code>I__am__ inside code</code></p>")]
        [TestCase("`__inside__`", ExpectedResult = "<p><code>__inside__</code></p>")]
        public string ParseDoubleUnderscoresInsideCode_AsJustCode(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("__test__`abc", ExpectedResult = "<p><strong>test</strong>`abc</p>")]
        [TestCase("`__inside__", ExpectedResult = "<p>`<strong>inside</strong></p>")]
        public string ParseDoubleUnderscoresInsideUnpairedCode_AsStrong(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("_ d_", ExpectedResult = "<p>_ d_</p>")]
        [TestCase("_a _", ExpectedResult = "<p>_a _</p>")]
        [TestCase("__ d__", ExpectedResult = "<p>__ d__</p>")]
        [TestCase("__a __", ExpectedResult = "<p>__a __</p>")]
        [TestCase("_ abc_abc_", ExpectedResult = "<p>_ abc<em>abc</em></p>")]
        public string UnworkingTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [Test]
        public void TestPerformance()
        {
            const string data = " _A_ __B__ _C_ `_D_`";
            var firstText = data.RepeatString(100);
            var secondText = data.RepeatString(500);

            var firstTime = GetRenderingTime(firstText);
            var secondTime = GetRenderingTime(secondText);

            Assert.IsTrue(secondTime / firstTime <= 6);
        }

        private long GetRenderingTime(string text)
        {
            var watch = new Stopwatch();
            watch.Start();
            mdProcessor.RenderToHtml(text);
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }
    }
}
