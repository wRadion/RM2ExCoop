namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvDelay : BehaviorCommand
    {
        // Delay the behavior script for a certain number of frames.
        public BhvDelay() : base(0x01, "DELAY") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt16();
            ushort num = bin.ReadUInt16();

            return new dynamic[] { num };
        }
    }
}
