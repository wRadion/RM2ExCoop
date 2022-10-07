using g3;

namespace RM2ExCoop.RM2C
{
    internal class ColTriangle
    {
        public ushort VertId1;
        public ushort VertId2;
        public ushort VertId3;
        public ushort SpecialParam;

        public ushort[] VertIds => new ushort[] { VertId1, VertId2, VertId3 };

        public static ColTriangle ReadRom(Rom rom, long start, bool special)
        {
            return new ColTriangle(
                rom.GetUInt16(start),
                rom.GetUInt16(start + 2),
                rom.GetUInt16(start + 4),
                special ? rom.GetUInt16(start + 6) : (ushort)0
            );
        }

        public ColTriangle(ushort vertId1, ushort vertId2, ushort vertId3, ushort specialParam = 0)
        {
            VertId1 = vertId1;
            VertId2 = vertId2;
            VertId3 = vertId3;
            SpecialParam = specialParam;
        }

        public ColTriangle Offset(ushort offset)
        {
            VertId1 += offset;
            VertId2 += offset;
            VertId3 += offset;
            return this;
        }

        public void CheckNorm(Rom rom, long start)
        {
            var v1 = (Vector3d)Vector3iUtil.ReadRom(rom, start + VertId1 * 6);
            var v2 = (Vector3d)Vector3iUtil.ReadRom(rom, start + VertId2 * 6);
            var v3 = (Vector3d)Vector3iUtil.ReadRom(rom, start + VertId3 * 6);

            var cp = (v2 - v1).Cross(v3 - v1);

            // TODO: Remove this cast to have proper calculation? (RM2C isn't doing it)
            if ((int)cp.y > 0)
                return;

            (VertId3, VertId2) = (VertId2, VertId3);
        }

        public string ToString(bool withParam = false)
        {
            if (withParam)
                return $"{VertId1}, {VertId2}, {VertId3}, {SpecialParam}";
            else
                return $"{VertId1}, {VertId2}, {VertId3}";
        }
    }
}
