namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetScissor : F3DCommand
    {
        public ushort XStart;
        public ushort YStart;
        public byte Mode;
        public ushort XEnd;
        public ushort YEnd;

        public G_SetScissor(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            XStart = bin.ReadUInt16(12);
            YStart = bin.ReadUInt16(12);
            bin.Pad(4);
            Mode = bin.ReadByte(4);
            XEnd = bin.ReadUInt16(12);
            YEnd = bin.ReadUInt16(12);
        }

        protected override dynamic[] GetArgs()
        {
            string modeStr = Mode switch
            {
                0 => "G_SC_NON_INTERLACE",
                2 => "G_SC_EVEN_INTERLACE",
                3 => "G_SC_ODD_INTERLACE",
                _ => "invalid mode"
            };

            return new dynamic[] { XStart, YStart, modeStr, XEnd, YEnd };
        }
    }
}
