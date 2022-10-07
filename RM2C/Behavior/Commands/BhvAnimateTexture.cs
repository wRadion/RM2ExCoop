namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvAnimateTexture : BehaviorCommand
    {
        // Animates an object using texture animation. <field> is always set to oAnimState.
        public BhvAnimateTexture() : base(0x34, "ANIMATE_TEXTURE", BehaviorParamType.FIELD) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte field = bin.ReadByte();
            ushort rate = bin.ReadUInt16();

            return new dynamic[] { field, rate };
        }
    }
}
