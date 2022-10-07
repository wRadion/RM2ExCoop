namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvBeginLoop : BhvNone
    {
        // Marks the beginning of an infinite loop.
        public BhvBeginLoop() : base(0x08, "BEGIN_LOOP") { }
    }
}
