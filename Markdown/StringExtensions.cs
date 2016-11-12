namespace Markdown
{
    static class StringExtensions
    {
        public static bool IsNotSubstringStartingFrom(this string str, string substr, int pos)
        {
            var cantFitInString = pos + substr.Length > str.Length;
            if (cantFitInString)
            {
                return true;
            }
            var nextSubstringIsNotSubstr = str.Substring(pos, substr.Length) != substr;
            return nextSubstringIsNotSubstr;
        }
    }
}
