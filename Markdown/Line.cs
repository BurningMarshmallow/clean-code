namespace Markdown
{
    public class Line
    {
        public readonly LineType LineType;
        public readonly string LineValue;
        public readonly string LineTypeOpeningTag;
        public readonly string LineTypeClosingTag;


        public Line(string lineValue, LineType lineType, string lineTypeOpeningTag, string lineTypeClosingTag)
        {
            LineValue = lineValue;
            LineType = lineType;
            LineTypeOpeningTag = lineTypeOpeningTag;
            LineTypeClosingTag = lineTypeClosingTag;
        }
    }
}
