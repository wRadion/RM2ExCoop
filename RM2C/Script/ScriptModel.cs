namespace RM2ExCoop.RM2C
{
    internal class ScriptModel
    {
        public uint Segment;
        public string Label;
        public string Type;
        public uint RomAddr;
        public uint SegAddr;
        public int Id;
        public string Folder;
        public Script Script;

        public ScriptModel(uint segment, string label, string type, uint romAddr, uint segAddr, int id, string folder, Script script)
        {
            Segment = segment;
            Label = label;
            Type = type;
            RomAddr = romAddr;
            SegAddr = segAddr;
            Id = id;
            Folder = folder;
            Script = script;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            if (obj is not ScriptModel other)
                return false;
            return Segment == other.Segment && Label == other.Label && Type == other.Type && RomAddr == other.RomAddr &&
                SegAddr == other.SegAddr && Id == other.Id && Folder == other.Folder && Script == other.Script;
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}
