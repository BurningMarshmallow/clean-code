namespace Markdown
{
    public class Tag
    {
        public string TagValue { get; }
        public string TagRepresentation { get; }
        public bool DigitsNotAllowed { get; }
        public int Bias;

        public Tag(string tagValue, string tagRepresentation, int bias = 1, bool digitsNotAllowed=true)
        {
            TagValue = tagValue;
            TagRepresentation = tagRepresentation;
            DigitsNotAllowed = digitsNotAllowed;
            Bias = bias;
        }
    }
}
