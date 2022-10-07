namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSumInt : BehaviorCommand
    {
        // Sets the destination integer field to the sum of the values of the given integer field. Unused
        public BhvSumInt() : base(0x20, "SUM_INT", BehaviorParamType.FIELD3) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte fieldDst = bin.ReadByte();
            byte fieldSrc1 = bin.ReadByte();
            byte fieldSrc2 = bin.ReadByte();

            return new dynamic[] { fieldDst, fieldSrc1, fieldSrc2 };
        }
    }
}
