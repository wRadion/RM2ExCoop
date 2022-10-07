namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvCmdNop4 : BehaviorCommand
    {
        // No operation. Unused.
        public BhvCmdNop4() : base(0x24, "CMD_NOP_4") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            ushort value = bin.ReadUInt16();

            return new dynamic[] { field, value };
        }
    }
}
