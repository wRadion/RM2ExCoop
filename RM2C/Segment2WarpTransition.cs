namespace RM2ExCoop.RM2C
{
    internal class Segment2WarpTransition
    {
        public uint Start;
        public string Name;
        public (int, int) Size;
        public uint Length;

        public Segment2WarpTransition(uint start, string name, (int, int) size, uint length)
        {
            Start = start;
            Name = name;
            Size = size;
            Length = length;
        }
    }
}
