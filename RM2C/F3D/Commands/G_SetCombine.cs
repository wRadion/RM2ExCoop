using System.Collections.Generic;

namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SetCombine : F3DCommand
    {
        public class Color
        {
            public dynamic Color1;
            public dynamic Color2;
            public dynamic Alpha1;
            public dynamic Alpha2;

            public Color()
            {
                Color1 = (byte)0;
                Color2 = (byte)0;
                Alpha1 = (byte)0;
                Alpha2 = (byte)0;
            }
        }
        public Color A;
        public Color B;
        public Color C;
        public Color D;

        public G_SetCombine(byte code, string name)
            : base(code, name)
        {
            A = new();
            B = new();
            C = new();
            D = new();
        }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            A.Color1 = bin.ReadByte(4); // a
            C.Color1 = bin.ReadByte(5); // b
            A.Alpha1 = bin.ReadByte(3); // c
            C.Alpha1 = bin.ReadByte(3); // d
            A.Color2 = bin.ReadByte(4); // e
            C.Color2 = bin.ReadByte(5); // f
            B.Color1 = bin.ReadByte(4); // g
            B.Color2 = bin.ReadByte(4); // h
            A.Alpha2 = bin.ReadByte(3); // i
            C.Alpha2 = bin.ReadByte(3); // j
            D.Color1 = bin.ReadByte(3); // k
            B.Alpha1 = bin.ReadByte(3); // l
            D.Alpha1 = bin.ReadByte(3); // m
            D.Color2 = bin.ReadByte(3); // n
            B.Alpha2 = bin.ReadByte(3); // o
            D.Alpha2 = bin.ReadByte(3); // p

            Dictionary<int, dynamic> basic = new() { { 1, "TEXEL0" }, { 2, "TEXEL1" }, { 3, "PRIMITIVE" }, { 4, "SHADE" }, { 5, "ENVIRONMENT" } };
            Dictionary<int, dynamic> basicA = basic;
            Dictionary<int, dynamic> combined = new() { { 0, "COMBINED" } };
            Dictionary<int, dynamic> combinedA = combined;
            Dictionary<int, dynamic> one = new() { { 6, 1 } };
            Dictionary<int, dynamic> c = new()
            {
                { 6, "SCALE" },
                { 7, "Combined_ALPHA" },
                { 8, "TEXEL0_ALPHA" },
                { 9, "TEXEL1_ALPHA" },
                { 10, "PRIMITIVE_ALPHA" },
                { 11, "SHADE_ALPHA" },
                { 12, "ENVIRONMENT_ALPHA" },
                { 13, "LOD_FRACTION" },
                { 14, "PRIM_LOD_FRACTION" },
                { 15, "K5" }
            };

            // A color = basic + combined + 7 as noise + one
            // B color = basic + combined + 6 as key center + 7 as key4
            // C color = basic + combined + C
            // D color = basic + combined + one
            // A alpha = basicA + combinedA + one
            // B alpha = A alpha
            // C alpha = basic + one + 0 as Lod fraction
            // D alpha = A alpha
            // Zero will be default aka out of range
            Dictionary<int, dynamic> aColorMode = Utils.MergeDict(basic, combined, one, new() { { 7, "Noise" } });
            Dictionary<int, dynamic> bColorMode = Utils.MergeDict(basic, combined, new() { { 6, "CENTER" }, { 7, "K4" } });
            Dictionary<int, dynamic> cColorMode = Utils.MergeDict(basic, combined, c);
            Dictionary<int, dynamic> dColorMode = Utils.MergeDict(basic, combined, one);

            Dictionary<int, dynamic> aAlphaMode = Utils.MergeDict(basicA, combinedA, one);
            Dictionary<int, dynamic> bAlphaMode = aAlphaMode;
            Dictionary<int, dynamic> cAlphaMode = Utils.MergeDict(basicA, new() { { 0, "LOD_FRACTION" } });
            Dictionary<int, dynamic> dAlphaMode = aAlphaMode;

            A.Color1 = aColorMode.GetValueOrDefault((int)A.Color1, 0);
            A.Color2 = aColorMode.GetValueOrDefault((int)A.Color2, 0);
            B.Color1 = bColorMode.GetValueOrDefault((int)B.Color1, 0);
            B.Color2 = bColorMode.GetValueOrDefault((int)B.Color2, 0);
            C.Color1 = cColorMode.GetValueOrDefault((int)C.Color1, 0);
            C.Color2 = cColorMode.GetValueOrDefault((int)C.Color2, 0);
            D.Color1 = dColorMode.GetValueOrDefault((int)D.Color1, 0);
            D.Color2 = dColorMode.GetValueOrDefault((int)D.Color2, 0);

            A.Alpha1 = aAlphaMode.GetValueOrDefault((int)A.Alpha1, 0);
            A.Alpha2 = aAlphaMode.GetValueOrDefault((int)A.Alpha2, 0);
            B.Alpha1 = bAlphaMode.GetValueOrDefault((int)B.Alpha1, 0);
            B.Alpha2 = bAlphaMode.GetValueOrDefault((int)B.Alpha2, 0);
            C.Alpha1 = cAlphaMode.GetValueOrDefault((int)C.Alpha1, 0);
            C.Alpha2 = cAlphaMode.GetValueOrDefault((int)C.Alpha2, 0);
            D.Alpha1 = dAlphaMode.GetValueOrDefault((int)D.Alpha1, 0);
            D.Alpha2 = dAlphaMode.GetValueOrDefault((int)D.Alpha2, 0);

            if (Globals.Cycle == 1)
            {
                // Get rid of combined by making assumptions about common combine types
                if (A.Color1.ToString() == "COMBINED")
                    A.Color1 = "TEXEL0";
                if (B.Color1.ToString() == "COMBINED")
                    B.Color1 = 0;
                if (C.Color1.ToString() == "COMBINED")
                    C.Color1 = "SHADE";
                if (D.Color1.ToString() == "COMBINED")
                    D.Color1 = 0;

                if (A.Alpha1.ToString() == "COMBINED")
                    A.Alpha1 = "TEXEL0";
                if (B.Alpha1.ToString() == "COMBINED")
                    B.Alpha1 = 0;
                if (C.Alpha1.ToString() == "COMBINED")
                    C.Alpha1 = "SHADE";
                if (D.Alpha1.ToString() == "COMBINED")
                    D.Alpha1 = 0;

                // Set 2 cycle values to same as 1 cycle values
                A.Color2 = A.Color1;
                B.Color2 = B.Color1;
                C.Color2 = C.Color1;
                D.Color2 = D.Color1;

                A.Alpha2 = A.Alpha1;
                B.Alpha2 = B.Alpha1;
                C.Alpha2 = C.Alpha1;
                D.Alpha2 = D.Alpha1;
            }
        }

        protected override dynamic[] GetArgs()
        {
            return new dynamic[]
            {
                A.Color1, B.Color1, C.Color1, D.Color1, A.Alpha1, B.Alpha1, C.Alpha1, D.Alpha1,
                A.Color2, B.Color2, C.Color2, D.Color2, A.Alpha2, B.Alpha2, C.Alpha2, D.Alpha2
            };
        }
    }
}
