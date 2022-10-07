namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_RDPHalf2 : F3DCommand
    {
        public uint Bits;

        public G_RDPHalf2(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(24);
            Bits = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Bits };
    }
}
