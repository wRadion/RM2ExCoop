namespace RM2ExCoop.RM2C
{
    internal class AddressRange
    {
        public uint Start;
        public uint End;

        public bool IsSet { get; private set; }
        public bool IsValid { get; private set; }

        public AddressRange(uint start = 0, uint end = 0)
        {
            Start = start;
            End = end;

            IsSet = Start != 0 || End != 0;
            IsValid = Start < End;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            if (obj is not AddressRange other)
                return false;
            return Start == other.Start && End == other.End;
        }

        public override int GetHashCode() => Start.GetHashCode() ^ End.GetHashCode();
    }
}
