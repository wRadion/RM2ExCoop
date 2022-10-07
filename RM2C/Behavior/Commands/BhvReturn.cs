namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvReturn : BhvNone
    {
        // Jumps back to the behavior command stores in the object's stack.
        public BhvReturn() : base(0x03, "RETURN") { }
    }
}
