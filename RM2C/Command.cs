namespace RM2ExCoop.RM2C
{
    internal struct Command
    {
        public byte Cmd;
        public int Length;
        public BitStream Args;
        public uint Start;

        public Command(byte cmd, int length, byte[] args, uint start)
        {
            Cmd = cmd;
            Length = length;
            Args = new(args);
            Start = start;
        }

        public override string ToString()
        {
            return string.Format($"({Cmd}, {Length}, ({string.Join(", ", Args)}), {Start})");
        }
    }
}
