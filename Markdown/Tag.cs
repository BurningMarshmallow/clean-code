namespace Markdown
{
    public class Tag
    {
        public string TagValue { get; }
        public string TagRepresentation { get; }
        public int Bias { get; set; }

        public Tag(string tagValue, string tagRepresentation, int bias = 1)
        {
            TagValue = tagValue;
            TagRepresentation = tagRepresentation;
            Bias = bias;
        }
    }
}
