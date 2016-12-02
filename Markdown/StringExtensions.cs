using System;
using System.Text;

namespace Markdown
{
    static class StringExtensions
    {
        public static string RepeatString(this string str, int numberOfRepeats)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < numberOfRepeats; i++)
                stringBuilder.Append(str);
            return stringBuilder.ToString();
        }

        public static bool IsNotSubstringStartingFrom(this string str, string substr, int pos)
        {
            var cantFitInString = pos + substr.Length > str.Length;
            if (cantFitInString)
            {
                return true;
            }

            var nextSubstringIsNotSubstr = str.IndexOf(substr, pos, substr.Length, StringComparison.Ordinal) == -1;
            return nextSubstringIsNotSubstr;
        }
    }
}
