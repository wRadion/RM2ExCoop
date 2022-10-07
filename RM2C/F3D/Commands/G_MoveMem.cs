namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_MoveMem : F3DCommand
    {
        public byte Index;
        public short Size;
        public uint Segment;
        public int FuckGbi; // idk
        public string Light = string.Empty;

        public G_MoveMem(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Index = bin.ReadByte();
            Size = bin.ReadInt16();
            Segment = bin.ReadUInt32();
            FuckGbi = Index == 0x88 ? 2 : 1;
            Light = $"&Light_{idPrefix}{Utils.Hex(Segment)}.col";
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Light, FuckGbi };
    }
}
