namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetInteractType : BehaviorCommand
    {
        // Sets the object's interact type.
        public BhvSetInteractType() : base(0x2F, "SET_INTERACT_TYPE") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint type = bin.ReadUInt32();

            return new dynamic[] { type };
        }
    }
}
