namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvBegin : BehaviorCommand
    {
        // Defines the start of the behavior script as well as the object list the object belongs to.
        // Has some special behavior for certain objects.
        public BhvBegin() : base(0x00, "BEGIN", BehaviorParamType.LIST) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte objList = bin.ReadByte();
            _ = bin.ReadUInt16();

            return new dynamic[] { objList };
        }
    }
}
