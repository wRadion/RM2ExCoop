namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_LoadBlock : F3DCommand
    {
        public ushort SStart;
        public ushort TStart;
        public byte Tile;
        public ushort Texels;
        public ushort Dxt;

        public G_LoadBlock(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            SStart = bin.ReadUInt16(12);
            TStart = bin.ReadUInt16(12);
            bin.Pad(4);
            Tile = bin.ReadByte(4);
            Texels = bin.ReadUInt16(12);
            Dxt = bin.ReadUInt16(12);
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Tile, SStart, TStart, Texels, Dxt };
    }
}
