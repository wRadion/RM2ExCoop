namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetRandomFloat : BehaviorCommand
    {
        // Sets the specified field to a random float in the given range.
        public BhvSetRandomFloat() : base(0x14, "SET_RANDOM_FLOAT", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            ushort min = bin.ReadUInt16();
            ushort range = bin.ReadUInt16();
            _ = bin.ReadUInt16();

            return new dynamic[] { field, min, range };
        }
    }
}
