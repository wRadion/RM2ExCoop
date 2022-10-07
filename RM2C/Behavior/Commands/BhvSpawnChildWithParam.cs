namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSpawnChildWithParam : BehaviorCommand
    {
        // Spawns a child object with the specified model and behavior plus a behavior param.
        public BhvSpawnChildWithParam() : base(0x29, "SPAWN_CHILD_WITH_PARAM") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            _ = bin.ReadByte();
            ushort bhvParam = bin.ReadUInt16();
            uint modelId = bin.ReadUInt32();
            uint behavior = bin.ReadUInt32();

            return new dynamic[] { bhvParam, modelId, behavior };
        }
    }
}
