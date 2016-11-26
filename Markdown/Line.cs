namespace Markdown
{
    public class Line
    {
        public readonly LineType Type;
        public readonly string Value;
        public readonly string OpeningTag;
        public readonly string ClosingTag;


        public Line(string value, LineType type, string openingTag, string closingTag)
        {
            Value = value;
            Type = type;
            OpeningTag = openingTag;
            ClosingTag = closingTag;
        }
    }
}
