namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvLoadCollisionData : BehaviorCommand
    {
        // Loads collision data for the object.
        public BhvLoadCollisionData() : base(0x2A, "LOAD_COLLISION_DATA", BehaviorParamType.COL) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint collisionData = bin.ReadUInt32();

            return new dynamic[] { collisionData };
        }
    }
}
