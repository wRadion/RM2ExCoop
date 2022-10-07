namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetConvert : F3DCommand
    {
        public short K0;
        public short K1;
        public short K2;
        public short K3;
        public short K4;
        public short K5;

        public G_SetConvert(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(2);
            K0 = bin.ReadInt16(9);
            K1 = bin.ReadInt16(9);
            K2 = bin.ReadInt16(9);
            K3 = bin.ReadInt16(9);
            K4 = bin.ReadInt16(9);
            K5 = bin.ReadInt16(9);
        }

        protected override dynamic[] GetArgs() => new dynamic[] { K0, K1, K2, K3, K4, K5 };
    }
}
