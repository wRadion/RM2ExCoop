namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetCImg : F3DCommand
    {
        public byte Fmt;
        public byte BitSize;
        public ushort Width;
        public uint Addr;

        public G_SetCImg(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Fmt = bin.ReadByte(3);
            BitSize = bin.ReadByte(2);
            Width = bin.ReadUInt16(12);
            Addr = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Fmt, BitSize, Width, Addr };
    }
}
