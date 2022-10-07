namespace RM2ExCoop.RM2C
{
    internal class WarpInstant : Warp
    {
        public byte CollisionId;
        public byte DestAreaId;
        public short DestX;
        public short DestY;
        public short DestZ;

        public WarpInstant(byte collisionId, byte destAreaId, short destX, short destY, short destZ)
        {
            CollisionId = collisionId;
            DestAreaId = destAreaId;
            DestX = destX;
            DestY = destY;
            DestZ = destZ;
        }

        public override string ToString() => $"{CollisionId},{DestAreaId},{DestX},{DestY},{DestZ}";
    }
}
