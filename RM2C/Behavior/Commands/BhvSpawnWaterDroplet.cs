namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSpawnWaterDroplet : BehaviorCommand
    {
        // Spawns a water droplet with the given parameters.
        public BhvSpawnWaterDroplet() : base(0x37, "SPAWN_WATER_DROPLET") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            uint dropletParams = bin.ReadUInt32();

            return new dynamic[] { dropletParams };
        }
    }
}
