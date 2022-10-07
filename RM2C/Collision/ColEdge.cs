namespace RM2ExCoop.RM2C
{
    internal class ColEdge
    {
        public ushort VertId1;
        public ushort VertId2;

        public ColEdge(ushort vertId1, ushort vertId2)
        {
            VertId1 = vertId1;
            VertId2 = vertId2;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not ColEdge other)
                return false;

            return (VertId1 == other.VertId1 && VertId2 == other.VertId2) ||
                (VertId1 == other.VertId2 && VertId2 == other.VertId1);
        }

        public override int GetHashCode() => VertId1 & VertId2;
    }
}
