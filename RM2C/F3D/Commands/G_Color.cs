namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_Color : F3DCommand
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public G_Color(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(24);
            R = bin.ReadByte();
            G = bin.ReadByte();
            B = bin.ReadByte();
            A = bin.ReadByte();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { R, G, B, A };
    }
}
