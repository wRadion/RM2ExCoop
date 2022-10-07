namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_LoadTLUT : F3DCommand
    {
        public byte Tile;
        public ushort Color;

        public G_LoadTLUT(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(28);
            Tile = bin.ReadByte(4);
            Color =  bin.ReadUInt16(12);
            bin.Pad(12);
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Tile, ((Color >> 2) & 0x3FF) + 1 };
    }
}
