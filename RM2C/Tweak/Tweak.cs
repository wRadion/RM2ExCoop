namespace RM2ExCoop.RM2C
{
    public enum TweakType { INHERIT, DOUBLE, FLOATUPPER, GFXRECTLEFT, GFXRECTRIGHT };

    internal abstract class Tweak
    {
        public string Name { get; set; }
        public TweakType Type { get; set; }
        public int[] Offsets { get; set; }

        public Tweak(string name, TweakType type, params int[] offsets)
        {
            Name = name;
            Type = type;
            Offsets = offsets;
        }

        public abstract string GetStrValue(Rom rom);
    }
}
