namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSpawnChild : BehaviorCommand
    {
        // Spawns a child object with the specified model and behavior
        public BhvSpawnChild() : base(0x1C, "SPAWN_CHILD") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint modelId = bin.ReadUInt32();
            uint behavior = bin.ReadUInt32();

            return new dynamic[] { modelId, behavior };
        }
    }
}
