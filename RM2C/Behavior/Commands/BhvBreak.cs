namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvBreak : BhvNone
    {
        // Exits the behavior script.
        // Often used to end behavior scripts that do not contain an infinite loop.
        public BhvBreak() : base(0x0A, "BREAK") { }
    }
}
