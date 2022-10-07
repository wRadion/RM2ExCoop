namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvEndLoop : BhvNone
    {
        // Marks the end of an infinite loop.
        public BhvEndLoop() : base(0x09, "END_LOOP") { }
    }
}
