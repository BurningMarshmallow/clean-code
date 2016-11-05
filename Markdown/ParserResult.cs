using System;
using System.Collections.Generic;

namespace Markdown
{
    public class ParserResult
    {
        public readonly string[] ParsedData;
        public readonly List<Tuple<int, int>> Ranges;

        public ParserResult(string[] data, List<Tuple<int, int>> ranges)
        {
            ParsedData = data;
            Ranges = ranges;
        }
    }
}
