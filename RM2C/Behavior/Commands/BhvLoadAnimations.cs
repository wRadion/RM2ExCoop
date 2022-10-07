namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvLoadAnimations : BehaviorCommand
    {
        // Loads the animations for the object. <field> is always set to oAnimations.
        public BhvLoadAnimations() : base(0x27, "LOAD_ANIMATIONS", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            ushort field = bin.ReadUInt16();
            _ = bin.ReadUInt16();
            uint anims = bin.ReadUInt32();

            return new dynamic[] { field, anims };
        }
    }
}
