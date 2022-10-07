namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_DL : F3DCommand
    {
        public byte Store;
        public uint Segment;
        public string DL = string.Empty;

        public G_DL(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Store = bin.ReadByte();
            bin.Pad(16);
            Segment = bin.ReadUInt32();
            DL = $"DL_{idPrefix}{Utils.Hex(Segment)}";
        }

        protected override dynamic[] GetArgs() => new dynamic[] { DL };

        public override void Setup()
        {
            if (Store != 1)
                Name = "gsSPDisplayList";
        }
    }
}
