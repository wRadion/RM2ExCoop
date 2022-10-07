namespace RM2ExCoop.RM2C
{
    internal class WarpConnected : Warp
    {
        public byte Id;
        public byte DestLevelId;
        public byte DestAreaId;
        public byte DestWarpId;
        public byte Flags;

        public WarpConnected(byte id, byte destLevelId, byte destAreaId, byte destWarpId, byte flags)
        {
            Id = id;
            DestLevelId = destLevelId;
            DestAreaId = destAreaId;
            DestWarpId = destWarpId;
            Flags = flags;
        }

        public override string ToString() => $"{Id},{DestLevelId},{DestAreaId},{DestWarpId},{Flags}";
    }
}
