namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetIntRandRshift : BehaviorCommand
    {
        // Gets a random short, right shifts it the specified amount and adds min to it, then sets the specified field to that value.
        public BhvSetIntRandRshift() : base(0x13, "SET_INT_RAND_RSHIFT", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            ushort min = bin.ReadUInt16();
            ushort rshift = bin.ReadUInt16();
            _ = bin.ReadUInt16();

            return new dynamic[] { field, min, rshift };
        }
    }
}
