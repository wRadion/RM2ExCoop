namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetPrimColor : F3DCommand
    {
        public byte Min;
        public byte Fraction;
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public G_SetPrimColor(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(24);
            Min = bin.ReadByte();
            Fraction = bin.ReadByte();
            R = bin.ReadByte();
            G = bin.ReadByte();
            B = bin.ReadByte();
            A = bin.ReadByte();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Min / 256, Fraction / 256, R, G, B, A };
    }
}
