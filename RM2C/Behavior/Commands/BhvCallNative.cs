namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvCallNative : BehaviorCommand
    {
        // Executes a native game function.
        public BhvCallNative() : base(0x0C, "CALL_NATIVE", BehaviorParamType.CALL) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint func = bin.ReadUInt32();

            return new dynamic[] { func };
        }
    }
}
