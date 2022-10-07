namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetModel : BehaviorCommand
    {
        // Sets the current model ID of the object.
        public BhvSetModel() : base(0x1B, "SET_MODEL") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            _ = bin.ReadByte();
            ushort modelId = bin.ReadUInt16();

            return new dynamic[] { modelId };
        }
    }
}
