namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvEndRepeatContinue : BhvNone
    {
        // Also marks the end of a repeating loop, but continues executing commands following the loop on the same frame.
        public BhvEndRepeatContinue() : base(0x07, "END_REPEAT_CONTINUE") { }
    }
}
