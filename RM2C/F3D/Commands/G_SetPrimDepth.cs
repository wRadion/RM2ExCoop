namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetPrimDepth : F3DCommand
    {
        public ushort ZVal;
        public ushort Depth;

        public G_SetPrimDepth(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(24);
            ZVal = bin.ReadUInt16();
            Depth = bin.ReadUInt16();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { ZVal, Depth };
    }
}
