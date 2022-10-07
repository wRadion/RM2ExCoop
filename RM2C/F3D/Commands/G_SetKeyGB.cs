namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetKeyGB : F3DCommand
    {
        public ushort GWidth;
        public ushort BWidth;
        public byte GInt;
        public byte GRecip;
        public byte BInt;
        public byte BRecip;

        public G_SetKeyGB(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            GWidth = bin.ReadUInt16(12);
            BWidth = bin.ReadUInt16(12);
            GInt = bin.ReadByte();
            GRecip = bin.ReadByte();
            BInt = bin.ReadByte();
            BRecip = bin.ReadByte();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { GWidth, BWidth, GInt, GRecip, BInt, BRecip };
    }
}
