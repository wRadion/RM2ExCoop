namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvScale : BehaviorCommand
    {
        // Sets the object's size to the specified percentage.
        public BhvScale() : base(0x32, "SCALE") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte unusedField = bin.ReadByte();
            ushort percent = bin.ReadUInt16();

            return new dynamic[] { unusedField, percent };
        }
    }
}
