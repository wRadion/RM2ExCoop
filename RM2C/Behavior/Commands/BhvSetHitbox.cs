namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetHitbox : BehaviorCommand
    {
        // Sets the size of the object's cylindrical hitbox.
        public BhvSetHitbox() : base(0x23, "SET_HITBOX") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            ushort radius = bin.ReadUInt16();
            ushort height = bin.ReadUInt16();

            return new dynamic[] { radius, height };
        }
    }
}
