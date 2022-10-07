namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_VTX : F3DCommand
    {
        public byte Num;
        public byte Start;
        public ushort Len;
        public uint Segment;
        public string VB = string.Empty;

        public G_VTX(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Num = bin.ReadByte(4);
            Start = bin.ReadByte(4);
            Len = bin.ReadUInt16(); // len
            Segment = bin.ReadUInt32();
            VB = $"VB_{idPrefix}{Utils.Hex(Segment)}";
        }

        protected override dynamic[] GetArgs() => new dynamic[] { VB, Num + 1, Start };
    }
}
