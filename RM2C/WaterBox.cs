using System;

namespace RM2ExCoop.RM2C
{
    internal class WaterBox : IComparable
    {
        public string Movtex;
        public int LevelId;
        public uint AreaId;
        public int MovtexType;

        public WaterBox(string movtex, int levelId, uint areaId, int movtexType)
        {
            Movtex = movtex;
            LevelId = levelId;
            AreaId = areaId;
            MovtexType = movtexType;
        }

        public int CompareTo(object? obj)
        {
            if (obj == null || obj is not WaterBox other)
                return 1;

            int cmp = LevelId.CompareTo(other.LevelId);
            if (cmp != 0)
                return cmp;
            cmp = AreaId.CompareTo(other.AreaId);
            if (cmp != 0)
                return cmp;
            return MovtexType.CompareTo(other.MovtexType);
        }
    }
}
