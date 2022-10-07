namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvBeginRepeatUnused : BehaviorCommand
    {
        // Unused. Marks the start of a loop that will repeat a certain number of times.
        // Uses a u8 as the argument, instead of a s16 like the other version does.
        public BhvBeginRepeatUnused() : base(0x26, "BEGIN_REPEAT_UNUSED") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte count = bin.ReadByte();
            _ = bin.ReadUInt16();

            return new dynamic[] { count };
        }
    }
}
