using System;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class TweakUshort : Tweak
    {
        public TweakUshort(string name, TweakType type, params int[] offsets) : base(name, type, offsets) { }

        public override string GetStrValue(Rom rom)
        {
            string[] values = Type switch
            {
                TweakType.INHERIT => Offsets.Select(o => rom.GetUInt16(o).ToString()).ToArray(),
                TweakType.DOUBLE => Offsets.Select(o => ((int)(rom.GetUInt16(o) * 1.5)).ToString()).ToArray(),
                TweakType.FLOATUPPER => Offsets.Select(o => BitConverter.ToSingle(BitConverter.GetBytes(rom.GetUInt16(o) << 16)).ToString() + ".0f").ToArray(),
                TweakType.GFXRECTLEFT => Offsets.Select(o => $"GFX_DIMENSIONS_RECT_FROM_LEFT_EDGE({rom.GetUInt16(o)})").ToArray(),
                TweakType.GFXRECTRIGHT => Offsets.Select(o => $"GFX_DIMENSIONS_RECT_FROM_RIGHT_EDGE({320 - rom.GetUInt16(o)})").ToArray(),
                _ => throw new NotImplementedException(),
            };

            return string.Join(", ", values);
        }
    }
}
