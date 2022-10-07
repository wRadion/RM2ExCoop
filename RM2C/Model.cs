using System;

namespace RM2ExCoop.RM2C
{
    internal class Model
    {
        public uint Segment;
        public string Type;
        public int? Layer;
        public uint RomAddr;
        public Script Script;

        public Model(uint segment, string type, int? layer, Script script)
        {
            Segment = segment;
            Type = type;
            Layer = layer;
            RomAddr = script.B2P(segment);
            Script = script;
        }
    }
}
