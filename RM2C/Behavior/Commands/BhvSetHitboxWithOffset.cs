namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetHitboxWithOffset : BehaviorCommand
    {
        // Sets the size of the object's cylindrical hitbox, and applies a downwards offset.
        public BhvSetHitboxWithOffset() : base(0x2B, "SET_HITBOX_WITH_OFFSET") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            ushort radius = bin.ReadUInt16();
            ushort height = bin.ReadUInt16();
            ushort downOffset = bin.ReadUInt16();
            _ = bin.ReadUInt16();

            return new dynamic[] { radius, height, downOffset };
        }
    }
}
