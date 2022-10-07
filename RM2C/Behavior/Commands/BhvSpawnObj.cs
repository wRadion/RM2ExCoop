namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSpawnObj : BehaviorCommand
    {
        // Spawns a new object with the specified model and behavior.
        public BhvSpawnObj() : base(0x2C, "SPAWN_OBJ") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint modelId = bin.ReadUInt32();
            uint behavior = bin.ReadUInt32();

            return new dynamic[] { modelId, behavior };
        }
    }
}
