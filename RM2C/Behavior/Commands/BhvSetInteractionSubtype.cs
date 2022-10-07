namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetInteractionSubtype : BehaviorCommand
    {
        // Sets the object's interaction subtype. Unused.
        public BhvSetInteractionSubtype() : base(0x31, "SET_INTERACTION_SUBTYPE") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint subtype = bin.ReadUInt32();

            return new dynamic[] { subtype };
        }
    }
}
