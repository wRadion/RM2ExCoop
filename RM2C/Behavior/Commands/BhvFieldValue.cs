namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal abstract class BhvFieldValue : BehaviorCommand
    {
        protected BhvFieldValue(int msb, string name) : base(msb, name, BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            ushort value = bin.ReadUInt16();

            return new dynamic[] { field, value };
        }
    }
}
