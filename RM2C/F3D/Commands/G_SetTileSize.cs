namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetTileSize : F3DCommand
    {
        public ushort SStart;
        public ushort TStart;
        public byte Tile;
        public ushort Width;
        public ushort Height;

        public G_SetTileSize(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            SStart = bin.ReadUInt16(12);
            TStart = bin.ReadUInt16(12);
            bin.Pad(4);
            Tile = bin.ReadByte(4);
            Width = bin.ReadUInt16(12);
            Height = bin.ReadUInt16(12);
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Tile, SStart, TStart, Width, Height };
    }
}
