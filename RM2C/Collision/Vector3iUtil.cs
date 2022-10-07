using g3;

namespace RM2ExCoop.RM2C
{
    internal static class Vector3iUtil
    {
        public static Vector3i ReadRom(Rom rom, long start)
        {
            return new Vector3i(rom.GetInt16(start), rom.GetInt16(start + 2), rom.GetInt16(start + 4));
        }
    }
}
