namespace RM2ExCoop.RM2C
{
    internal struct Macro
    {
        public byte YRot;
        public ushort Preset;
        public ushort X;
        public ushort Y;
        public ushort Z;
        public ushort BParam;

        public Macro(byte yRot, ushort preset, ushort x, ushort y, ushort z, ushort bParam)
        {
            YRot = yRot;
            Preset = preset;
            X = x;
            Y = y;
            Z = z;
            BParam = bParam;
        }

        public override string ToString() => $"{Data.MacroNames[Preset]},{YRot},{X},{Y},{Z},{BParam}";
    }
}
