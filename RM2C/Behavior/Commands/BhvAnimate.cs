namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvAnimate : BehaviorCommand
    {
        // Begins animation and sets the object's current animation index to the specified value.
        public BhvAnimate() : base(0x28, "ANIMATE") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadByte();
            byte animIndex = bin.ReadByte();
            _ = bin.ReadUInt16();

            return new dynamic[] { animIndex };
        }
    }
}
