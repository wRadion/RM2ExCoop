namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetOtherModeH : F3DCommand
    {
        public byte Shift;
        public byte Bits;
        public uint Value;

        public string ValueStr = string.Empty;
        public string Enum = string.Empty;

        public G_SetOtherModeH(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(8);
            Shift = bin.ReadByte();
            Bits = bin.ReadByte();
            Value = bin.ReadUInt32();

            Enum = Shift switch
            {
                4 => "gsDPSetAlphaDither",
                6 => "gsDPSetColorDither",
                8 => "gsDPSetCombineKey",
                9 => "gsDPSetTextureConvert",
                12 => "gsDPSetTextureFilter",
                14 => "gsDPSetTextureLUT",
                16 => "gsDPSetTextureLOD",
                17 => "gsDPSetTextureDetail",
                19 => "gsDPSetTexturePersp",
                20 => "gsDPSetCycleType",
                22 => "gsDPSetColorDither",
                23 => "gsDPPipelineMode",
                _ => string.Empty
            };

            ValueStr = Shift switch
            {
                4 => Value switch
                {
                    0x00 => "G_AD_PATTERN",
                    0x10 => "G_AD_NOTPATTERN",
                    0x20 => "G_AD_NOISE",
                    0x30 => "G_AD_DISABLE",
                    _ => string.Empty
                },
                6 => Value switch
                {
                    0x00 => "G_CD_DISABLE",
                    0x40 => "G_CD_BAYER",
                    0x80 => "G_CD_NOISE",
                    _ => string.Empty
                },
                8 => Value switch
                {
                    0x000 => "G_CK_NONE",
                    0x100 => "G_CK_KEY",
                    _ => string.Empty
                },
                9 => Value switch
                {
                    0x000 => "G_TC_CONV",
                    0xA00 => "G_TC_FILTCONV",
                    0xC00 => "G_TC_FILT",
                    _ => string.Empty
                },
                12 => Value switch
                {
                    0x0000 => "G_TF_POINT",
                    0x2000 => "G_TF_BILERP",
                    0x3000 => "G_TF_AVERAGE",
                    _ => string.Empty
                },
                14 => Value switch
                {
                    0x0000 => "G_TT_NONE",
                    0x8000 => "G_TT_RGBA16",
                    0xC000 => "G_TT_IA16",
                    _ => string.Empty
                },
                16 => Value switch
                {
                    0x00000 => "G_TL_TILE",
                    0x10000 => "G_TL_LOD",
                    _ => string.Empty
                },
                17 => Value switch
                {
                    0x00000 => "G_TD_CLAMP",
                    0x20000 => "G_TD_SHARPEN",
                    0x40000 => "G_TD_DETAIL",
                    _ => string.Empty
                },
                19 => Value switch
                {
                    0x00000 => "G_TP_NONE",
                    0x80000 => "G_TP_PERSP",
                    _ => string.Empty
                },
                20 => Value switch
                {
                    0x000000 => "G_CYC_1CYCLE",
                    0x100000 => "G_CYC_2CYCLE",
                    0x200000 => "G_CYC_COPY",
                    0x300000 => "G_CYC_FILL",
                    _ => string.Empty
                },
                23 => Value switch
                {
                    0x000000 => "G_PM_1PRIMITIVE",
                    0x800000 => "G_PM_NPRIMITIVE",
                    _ => string.Empty
                },
                _ => Value.ToString()
            };

            if (Shift == 20)
            {
                if (Value == 1048576)
                    Globals.Cycle = 2;
                else if (Value == 0)
                    Globals.Cycle = 1;
            }
        }

        protected override dynamic[] GetArgs()
        {
            if (Enum.Length == 0)
                return new dynamic[] { Code, Shift, Bits, Value };

            if (ValueStr.Length == 0)
                return new dynamic[] { Value };

            return new dynamic[] { ValueStr };
        }

        public override void Setup()
        {
            if (Args.Length >= 4)
                return;

            Name = Enum;
        }
    }
}
