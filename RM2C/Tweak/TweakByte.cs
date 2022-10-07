using System;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class TweakByte : Tweak
    {
        public TweakByte(string name, TweakType type, params int[] offsets) : base(name, type, offsets) { }

        public override string GetStrValue(Rom rom)
        {
            string[] values = Type switch
            {
                TweakType.INHERIT => Offsets.Select(o => rom.GetByte(o).ToString()).ToArray(),
                TweakType.DOUBLE => Offsets.Select(o => ((int)(rom.GetByte(o) * 1.5)).ToString()).ToArray(),
                TweakType.GFXRECTLEFT => Offsets.Select(o => $"GFX_DIMENSIONS_RECT_FROM_LEFT_EDGE({rom.GetByte(o)})").ToArray(),
                TweakType.GFXRECTRIGHT => Offsets.Select(o => $"GFX_DIMENSIONS_RECT_FROM_RIGHT_EDGE({320 - rom.GetByte(o)})").ToArray(),
                _ => throw new NotImplementedException()
            };

            return string.Join(", ", values);
        }
    }
}
