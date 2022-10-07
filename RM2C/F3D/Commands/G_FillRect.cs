namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_FillRect : F3DCommand
    {
        public ushort XStart;
        public ushort YStart;
        public ushort XEnd;
        public ushort YEnd;

        public G_FillRect(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            XStart = bin.ReadUInt16(12);
            YStart = bin.ReadUInt16(12);
            bin.Pad(8);
            XEnd = bin.ReadUInt16(12);
            YEnd = bin.ReadUInt16(12);
        }

        protected override dynamic[] GetArgs() => new dynamic[] { XStart, YStart, XEnd, YEnd };
    }
}
