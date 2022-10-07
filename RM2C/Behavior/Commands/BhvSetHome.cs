namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetHome : BhvNone
    {
        // Sets the home position of the object to its current position.
        public BhvSetHome() : base(0x2D, "SET_HOME") { }
    }
}
