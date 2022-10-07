namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvDeactivate : BhvNone
    {
        // Exits the behavior script and despawns the object.
        // Often used to end behavior scripts that do not contain an infinite loop.
        public BhvDeactivate() : base(0x1D, "DEACTIVATE") { }
    }
}
