namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvDelayVar : BehaviorCommand
    {
        // Delays the behavior script for the number of frames given by the value of the specified field.
        public BhvDelayVar() : base(0x25, "DELAY_VAR", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            _ = bin.ReadUInt16();

            return new dynamic[] { field };
        }
    }
}
