namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetZImg : F3DCommand
    {
        public uint Addr;

        public G_SetZImg(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(24);
            Addr = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Addr };
    }
}
