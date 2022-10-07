namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_RDPSetOtherMode : F3DCommand
    {
        public uint Hi;
        public uint Lo;

        public G_RDPSetOtherMode(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Hi = bin.ReadUInt32(24);
            Lo = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Hi, Lo };
    }
}
