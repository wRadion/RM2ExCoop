namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_Texture : F3DCommand
    {
        public byte Mip;
        public byte Tile;
        public byte State;
        public ushort SScale;
        public ushort TScale;

        public G_Texture(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(10);
            Mip = bin.ReadByte(3);
            Tile = bin.ReadByte(3);
            State = bin.ReadByte();
            SScale = bin.ReadUInt16();
            TScale = bin.ReadUInt16();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { SScale, TScale, Mip, Tile, State };
    }
}
