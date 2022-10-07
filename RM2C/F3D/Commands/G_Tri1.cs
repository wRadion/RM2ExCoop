namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_Tri1 : F3DCommand
    {
        public byte V1;
        public byte V2;
        public byte V3;

        public G_Tri1(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(32);
            V1 = bin.ReadByte();
            V2 = bin.ReadByte();
            V3 = bin.ReadByte();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { V1 / 10, V2 / 10, V3 / 10, 0 };
    }
}
