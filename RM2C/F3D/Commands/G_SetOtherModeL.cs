namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetOtherModeL : F3DCommand
    {
        public byte Shift;
        public byte Bits;
        public uint Value;

        public string Enum = string.Empty;

        public bool FogShadeA = false;

        public G_SetOtherModeL(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(8);
            Shift = bin.ReadByte();
            Bits = bin.ReadByte();
            Value = bin.ReadUInt32();

            Enum = Shift switch
            {
                0 => "gsDPSetAlphaCompare",
                2 => "gsDPSetDepthSource",
                3 => "gsDPSetRenderMode",
                _ => string.Empty
            };
        }

        protected override dynamic[] GetArgs()
        {
            if (Enum.Length == 0)
                return new dynamic[] { Code, Shift, Bits, Value };

            if (Shift == 3)
            {
                if (Value == 0xC8112078)
                {
                    FogShadeA = true;
                    return new dynamic[] { "G_RM_FOG_SHADE_A", "G_RM_AA_ZB_OPA_SURF2" };
                }
                else if (Value == 0xC8113078)
                {
                    FogShadeA = true;
                    return new dynamic[] { "G_RM_FOG_SHADE_A", "G_RM_AA_ZB_TEX_EDGE2" };
                }
                return new dynamic[] { 0, Value };
                // return new dynamic[] { 0, 0 }; // Fixes noisy textures
            }

            return new dynamic[] { Value };
        }

        public override void Setup()
        {
            if (Args.Length >= 4)
                return;

            Name = Enum;
        }
    }
}
