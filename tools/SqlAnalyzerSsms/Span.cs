namespace SqlAnalyzerExtension
{
    public class Span
    {
        public Span(int from, int length)
        {
            From = from;
            Length = length;
        }

        public int From { get; }

        public int Length { get; }
    }
}
