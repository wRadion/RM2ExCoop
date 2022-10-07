namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_LoadTile : F3DCommand
    {
        public ushort SStart;
        public ushort TStart;
        public byte Tile;
        public ushort SEnd;
        public ushort TEnd;

        public G_LoadTile(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            SStart = bin.ReadUInt16(12);
            TStart = bin.ReadUInt16(12);
            bin.Pad(4);
            Tile = bin.ReadByte(4);
            SEnd = bin.ReadUInt16(12);
            TEnd = bin.ReadUInt16(12);
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Tile, SStart, TStart, SEnd, TEnd };
    }
}
