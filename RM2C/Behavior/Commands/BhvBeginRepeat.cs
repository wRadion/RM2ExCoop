namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvBeginRepeat : BehaviorCommand
    {
        // Marks the start of a loop that will repeat a certain number of times.
        public BhvBeginRepeat() : base(0x05, "BEGIN_REPEAT") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt16();
            ushort count = bin.ReadUInt16();

            return new dynamic[] { count };
        }
    }
}
