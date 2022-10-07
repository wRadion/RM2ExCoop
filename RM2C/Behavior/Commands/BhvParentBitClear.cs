namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvParentBitClear : BehaviorCommand
    {
        // Performs a bit clear on the object's parent's field with the specified value.
        // Used for clearing active particle flags from Mario's object.
        public BhvParentBitClear() : base(0x33, "PARENT_BIT_CLEAR", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            _ = bin.ReadUInt16();
            uint flags = bin.ReadUInt32();

            return new dynamic[] { field, flags };
        }
    }
}
