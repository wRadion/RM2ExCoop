namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvSetObjPhysics : BehaviorCommand
    {
        // Sets various parameters that the object uses for calculating physics.
        public BhvSetObjPhysics() : base(0x30, "SET_OBJ_PHYSICS") { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();
            ushort wallHitboxRadius = bin.ReadUInt16();
            ushort gravity = bin.ReadUInt16();
            ushort bouciness = bin.ReadUInt16();
            ushort dragStrength = bin.ReadUInt16();
            ushort friction = bin.ReadUInt16();
            ushort buoyancy = bin.ReadUInt16();
            ushort unused1 = bin.ReadUInt16();
            ushort unused2 = bin.ReadUInt16();

            return new dynamic[] { wallHitboxRadius, gravity, bouciness, dragStrength, friction, buoyancy, unused1, unused2 };
        }
    }
}
