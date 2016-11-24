using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Markdown
{
    [TestFixture]
    public class Md_Should
    {
        private Md mdProcessor;

        [SetUp]
        public void SetUp()
        {
            mdProcessor = new Md();
        }

        [TestCase("abc\r\ndef", ExpectedResult = "<p>abc</p>\r\n<p>def</p>", TestName = "Two consequent lines")]
        public string ParseMultiplesLines_AsDifferentParagraps(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("a\r\n\r\nb", ExpectedResult = "<p>a</p>\r\n<p>b</p>", TestName = "Two lines separated by two empty lines")]
        [TestCase("a\r\n\r\n\r\nb", ExpectedResult = "<p>a</p>\r\n<p>b</p>", TestName = "Two lines separated by three empty lines")]
        public string SkipMultipleEmptyLines(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("_f_\r\n_e_", ExpectedResult = "<p><em>f</em></p>\r\n<p><em>e</em></p>", TestName = "Single underscores in first line and single underscores in second")]
        [TestCase("__x__\r\n__y__", ExpectedResult = "<p><strong>x</strong></p>\r\n<p><strong>y</strong></p>", TestName = "Double underscores in first line and double underscores in second")]
        [TestCase("_mark_\r\n__down__", ExpectedResult = "<p><em>mark</em></p>\r\n<p><strong>down</strong></p>", TestName = "Single underscores in first line and double underscores in second")]
        [TestCase("__down__\r\n_mark_", ExpectedResult = "<p><strong>down</strong></p>\r\n<p><em>mark</em></p>", TestName = "Double underscores in first line and single underscores in second")]
        public string ParseUnderscoresInMultipleLines_AsExpected(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("__content__", ExpectedResult = "<p><strong style=\"test\">content</strong></p>", TestName = "Strong tags with style")]
        [TestCase("_markdown_", ExpectedResult = "<p><em style=\"test\">markdown</em></p>", TestName = "Em tags with style")]
        public string ParseUnderscores_ToTagsWithStyle(string text)
        {
            var mdProcessorWithStyle = new Md("", "test");
            var rendered = mdProcessorWithStyle.RenderToHtml(text);
            return rendered;
        }

        [TestCase("`stub`", ExpectedResult = "<p><code style=\"test\">stub</code></p>", TestName = "Code tags with style")]
        public string ParseBackticks_ToCodeTagsWithStyle(string text)
        {
            var mdProcessorWithStyle = new Md("", "test");
            var rendered = mdProcessorWithStyle.RenderToHtml(text);
            return rendered;
        }

        [TestCase("[Yandex](http://ya.ru/) link.",
            ExpectedResult = "<p><a href=\"http://ya.ru/\" style=\"search\">Yandex</a> link.</p>", TestName = "Link after some text")]
        public string ParseLink_AsHrefWithStyle(string text)
        {
            var mdProcessorWithStyle = new Md("", "search");
            var rendered = mdProcessorWithStyle.RenderToHtml(text);
            return rendered;
        }

        [TestCase("This is [an example](http://example.com/) inline link.",
            ExpectedResult = "<p>This is <a href=\"http://example.com/\">an example</a> inline link.</p>", TestName = "Link after some text")]
        [TestCase("[Google](http://google.com/)",
            ExpectedResult = "<p><a href=\"http://google.com/\">Google</a></p>", TestName = "Just link")]
        public string ParseAbsoluteLink_AsHref(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("[Random text](/random/189888abc) seems quite interesting!",
         ExpectedResult = "<p><a href=\"http://example.com/random/189888abc\">Random text</a> seems quite interesting!</p>", TestName = "Relative link at the beginning")]
        [TestCase("See my [About](/about/) page for details.",
         ExpectedResult = "<p>See my <a href=\"http://example.com/about/\">About</a> page for details.</p>", TestName = "Relative link at the middle")]
        public string ParseRelativeLink_AsHref(string text)
        {
            var mdProcessorWithBaseUrl = new Md("http://example.com");
            var rendered = mdProcessorWithBaseUrl.RenderToHtml(text);
            return rendered;
        }

        [TestCase("iamatest.STUB", ExpectedResult = "<p>iamatest.STUB</p>")]
        [TestCase("bla-bla-bla", ExpectedResult = "<p>bla-bla-bla</p>")]
        public string ParseSimpleText_AsParagraph(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("first_info_second", ExpectedResult = "<p>first<em>info</em>second</p>", TestName = "Em tags in the middle")]
        [TestCase("_WWW_", ExpectedResult = "<p><em>WWW</em></p>", TestName = "Just em tags")]
        public string ParseSingleUnderscores_ToEmTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("why__ooooo__why", ExpectedResult = "<p>why<strong>ooooo</strong>why</p>", TestName = "Strong tags in the middle")]
        [TestCase("__ABBA__", ExpectedResult = "<p><strong>ABBA</strong></p>", TestName = "Just strong tags")]
        public string ParseDoubleUnderscores_ToStrongTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("`CODE`", ExpectedResult = "<p><code>CODE</code></p>", TestName = "Just code tags")]
        public string ParseSingleQuotes_ToCodeTags(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase(@"\_shiEld\_", ExpectedResult = "<p>_shiEld_</p>", TestName = "Escape single underscores")]
        [TestCase(@"\_\_rapapa\_\_", ExpectedResult = "<p>__rapapa__</p>", TestName = "Escape double underscores")]
        [TestCase(@"\`apapa\`", ExpectedResult = "<p>`apapa`</p>", TestName = "Escape code tags")]
        public string ParseEscapedTags_AsSimpleText(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("__def__ghij_K_lm", ExpectedResult = "<p><strong>def</strong>ghij<em>K</em>lm</p>", TestName = "Single underscores after double underscores")]
        [TestCase("_xx_and__yy__cc", ExpectedResult = "<p><em>xx</em>and<strong>yy</strong>cc</p>", TestName = "Double underscores after single underscores")]
        public string ParseMultipleConsequentTags_OneByOne(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("__text_", ExpectedResult = "<p>__text_</p>", TestName = "Single underscore after double underscore")]
        [TestCase("_anothertext__", ExpectedResult = "<p>_anothertext__</p>", TestName = "Double underscore after single underscore")]
        public string ParseUnpairedTags_AsSimpleText(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("`_inactive_`", ExpectedResult = "<p><code>_inactive_</code></p>")]
        public string ParseSingleUnderscoresInsideCode_AsJustCode(string text)
        {
            var rendered = mdProcessor.RenderToHtml(text);
            return rendered;
        }

        [TestCase("some_text_`", ExpectedResult = "<p>some<em>text</em>`</p>")]
        [TestCase("`before_inside_", ExpectedResult = "<p>`before<em>inside</em></p>")]
        public string ParseSingleUnderscoresInsideUnpairedCode_AsEm(string text)
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

        [TestCase(100, 500)]
        [TestCase(39, 120)]
        [TestCase(50, 600)]
        [TestCase(200, 2000)]
        [TestCase(1000, 10000)]
        public void TestPerformance(int firstLen, int secondLen)
        {
            const string data = " _A_ __B__ _C_ `_D_`";
            var firstText = data.RepeatString(firstLen);
            var secondText = data.RepeatString(secondLen);

            var firstTime = GetRenderingTime(firstText);
            var secondTime = GetRenderingTime(secondText);

            var factor = secondLen / firstLen;

            Assert.IsTrue(secondTime / firstTime <= 9 * factor);
        }

        private long GetRenderingTime(string text)
        {
            var watch = new Stopwatch();
            watch.Start();
            mdProcessor.RenderToHtml(text);
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        [TestCase(3000)]
        [TestCase(4000)]
        public void TestPerformanceComparedToActivity(int numberOfTimes)
        {
            var builder = new StringBuilder();
            const string test = "_ABA_ __CCCC__ _D_ _R_";
            for (var i = 0; i < numberOfTimes; i++)
                builder.Append(test);
            var text = builder.ToString();
            var sw = Stopwatch.StartNew();
            var tmp = 0;
            foreach (var symbol in text)
            {
                for (var i = 0; i < 1000; i++)
                    tmp++;
            }

            mdProcessor.RenderToHtml(text);
            mdProcessor.RenderToHtml(text);
            mdProcessor.RenderToHtml(text);


            var linearTime = sw.ElapsedMilliseconds;
            sw.Restart();
            mdProcessor.RenderToHtml(text);
            var resultTime = sw.ElapsedMilliseconds;
            Assert.LessOrEqual(resultTime / linearTime, 20);
        }
    }
}
