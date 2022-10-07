namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetTile : F3DCommand
    {
        public byte Fmt;
        public byte BitSize;
        public ushort NumRows;
        public ushort Offset;
        public byte Tile;
        public byte Palette;
        public byte TFlag;
        public byte TMask;
        public byte TShift;
        public byte SFlag;
        public byte SMask;
        public byte SShift;

        public G_SetTile(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Fmt = bin.ReadByte(3);
            BitSize = bin.ReadByte(2);
            bin.Pad(1);
            NumRows = bin.ReadUInt16(9);
            Offset = bin.ReadUInt16(9);
            bin.Pad(5);
            Tile = bin.ReadByte(3);
            Palette = bin.ReadByte(4);
            TFlag = bin.ReadByte(2);
            TMask = bin.ReadByte(4);
            TShift = bin.ReadByte(4);
            SFlag = bin.ReadByte(2);
            SMask = bin.ReadByte(4);
            SShift = bin.ReadByte(4);
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Fmt, BitSize, NumRows, Offset, Tile, Palette, TFlag, TMask, TShift, SFlag, SMask, SShift };
    }
}
