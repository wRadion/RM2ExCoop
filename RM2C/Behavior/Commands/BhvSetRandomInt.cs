namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetRandomInt : BehaviorCommand
    {
        // Sets the specified field to a random integer in the given range.
        public BhvSetRandomInt() : base(0x15, "SET_RANDOM_INT", BehaviorParamType.FIELD) { }

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
