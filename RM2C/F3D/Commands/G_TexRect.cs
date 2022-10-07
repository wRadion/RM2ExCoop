namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_TexRect : F3DCommand
    {
        public ushort XStart;
        public ushort YStart;
        public byte Tile;
        public ushort XEnd;
        public ushort YEnd;
        public ushort SStart;
        public ushort TStart;
        public ushort DsDx;
        public ushort DtDy;

        public G_TexRect(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            XStart = bin.ReadUInt16(12);
            YStart = bin.ReadUInt16(12);
            bin.Pad(4);
            Tile = bin.ReadByte(4);
            XEnd = bin.ReadUInt16(12);
            YEnd = bin.ReadUInt16(12);
            bin.Pad(32);
            SStart = bin.ReadUInt16();
            TStart = bin.ReadUInt16();
            bin.Pad(32);
            DsDx = bin.ReadUInt16();
            DtDy = bin.ReadUInt16();
        }

        protected override dynamic[] GetArgs() =>  new dynamic[] { XStart, YStart, Tile, XEnd, YEnd, SStart, TStart, DsDx, DtDy };
    }
}
