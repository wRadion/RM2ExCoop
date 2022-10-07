namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvBitClear : BhvFieldValue
    {
        // Performs a bit clear with the specified short. Unused in favor of the 32-bit version.
        public BhvBitClear() : base(0x12, "BIT_CLEAR") { }
    }
}
