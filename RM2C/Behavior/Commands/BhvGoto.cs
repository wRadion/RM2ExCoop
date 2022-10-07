namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvGoto : BehaviorCommand
    {
        // Jumps to a new behavior script without saving anything.
        public BhvGoto() : base(0x04, "GOTO", BehaviorParamType.JUMP) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint addr = bin.ReadUInt32();

            return new dynamic[] { addr };
        }
    }
}
