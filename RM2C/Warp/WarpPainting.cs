namespace RM2ExCoop.RM2C
{
    internal class WarpPainting : WarpConnected
    {
        public WarpPainting(byte id, byte destLevelId, byte destAreaId, byte destWarpId, byte flags)
            : base(id, destLevelId, destAreaId, destWarpId, flags)
        {
        }
    }
}
