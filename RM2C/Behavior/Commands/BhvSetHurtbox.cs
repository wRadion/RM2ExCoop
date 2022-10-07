namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetHurtbox : BehaviorCommand
    {
        // Sets the size of the object's cylindrical hurtbox.
        public BhvSetHurtbox() : base(0x2E, "SET_HURTBOX") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            ushort radius = bin.ReadUInt16();
            ushort height = bin.ReadUInt16();

            return new dynamic[] { radius, height };
        }
    }
}
