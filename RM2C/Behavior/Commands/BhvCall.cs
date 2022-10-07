namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvCall : BehaviorCommand
    {
        // Jumps to a new behavior command and stores the return address in the object's stack.
        public BhvCall() : base(0x02, "CALL", BehaviorParamType.JUMP) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint addr = bin.ReadUInt32();

            return new dynamic[] { addr };
        }
    }
}
