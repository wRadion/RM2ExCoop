namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_ClearGeometryMode : F3DCommand
    {
        public uint Set;

        public G_ClearGeometryMode(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            bin.Pad(24);
            Set = bin.ReadUInt32();
        }

        protected override dynamic[] GetArgs() => new dynamic[] { Set == 0 ? 0 : F3D.CheckGeoMacro(Set), 0 };
    }
}
