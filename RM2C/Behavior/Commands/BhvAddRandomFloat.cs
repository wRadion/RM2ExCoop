namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvAddRandomFloat : BehaviorCommand
    {
        // Adds a random float in the given range to the specified field.
        public BhvAddRandomFloat() : base(0x16, "ADD_RANDOM_FLOAT", BehaviorParamType.FIELD) { }

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
