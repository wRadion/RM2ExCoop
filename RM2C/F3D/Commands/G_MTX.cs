namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_MTX : F3DCommand
    {
        public byte Param;
        public uint Segment;

        public G_MTX(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(16);
            Param = bin.ReadByte();
            Segment = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Param, Segment };
    }
}
