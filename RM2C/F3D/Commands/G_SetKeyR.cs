namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetKeyR : F3DCommand
    {
        public ushort RWidth;
        public byte RInt;
        public byte RRecip;

        public G_SetKeyR(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(28);
            RWidth = bin.ReadUInt16(12);
            RInt = bin.ReadByte();
            RRecip = bin.ReadByte();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { RWidth, RInt, RRecip };
    }
}
