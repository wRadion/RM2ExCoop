namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvDropToFloor : BhvNone
    {
        // Finds the floor triangle directly under the object and moves the object down to it.
        public BhvDropToFloor() : base(0x1E, "DROP_TO_FLOOR") { }
    }
}
