namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvOrInt : BhvFieldValue
    {
        // Performs a bitwise OR with the specified field and the given integer.
        // Usually used to set an object's flags.
        public BhvOrInt() : base(0x11, "OR_INT") { }
    }
}
