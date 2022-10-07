namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetIntUnused : BehaviorCommand
    {
        // Unused. Sets the specified field to an integer. Wastes 4 bytes of space for no reason at all.
        public BhvSetIntUnused() : base(0x36, "SET_INT_UNUSED", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            _ = bin.ReadUInt16();
            ushort value = bin.ReadUInt16();
            _ = bin.ReadUInt16();

            return new dynamic[] { field, value };
        }
    }
}
