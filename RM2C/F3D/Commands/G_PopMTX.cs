namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_PopMTX : F3DCommand
    {
        public uint Num;

        public G_PopMTX(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(24);
            Num = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Num / 64 };
    }
}
