namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_MoveWord : F3DCommand
    {
        public ushort Offset;
        public byte Index;
        public uint Value;

        public G_MoveWord(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            Offset = bin.ReadUInt16();
            Index = bin.ReadByte();
            Value = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs()
        {
            string indexStr = Index switch
            {
                0 => "G_MW_MATRIX",
                2 => "G_MW_NUMLIGHT",
                4 => "G_MW_CLIP",
                6 => "G_MW_SEGMENT",
                8 => "G_MW_FOG",
                10 => "G_MW_LIGHTCOL",
                12 => "G_MW_FORCEMTX",
                14 => "G_MW_PERSPNORM",
                _ => string.Empty
            };

            return new dynamic[] { indexStr.Length == 0 ? Index : indexStr, Offset, Value };
        }
    }
}
