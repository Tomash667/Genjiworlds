namespace Genjiworlds.Helpers
{
    public struct Range
    {
        public int from, to;

        public Range(int from, int to)
        {
            this.from = from;
            this.to = to;
        }

        public int Random()
        {
            return Utils.Random(from, to);
        }
    }
}
