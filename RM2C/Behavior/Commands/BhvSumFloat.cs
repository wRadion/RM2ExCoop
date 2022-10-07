namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSumFloat : BehaviorCommand
    {
        // Sets the destination float field to the sum of the values of the given float fields.
        public BhvSumFloat() : base(0x1F, "SUM_FLOAT", BehaviorParamType.FIELD3) { }

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
