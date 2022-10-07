namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetTImg : F3DCommand
    {
        public byte Fmt;
        public byte BitSize;
        public uint Segment;
        public string Texture = string.Empty;

        public G_SetTImg(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Fmt = bin.ReadByte(3);
            BitSize = bin.ReadByte(2);
            bin.Pad(19);
            Segment = bin.ReadUInt32();
            Texture = $"{idPrefix}_texture_{Segment:X8}";
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Fmt, BitSize, 1, Texture };
    }
}
