namespace RM2ExCoop.RM2C
{
    internal class BehaviorFunction
    {
        public uint RomAddr;
        public string BhvName;
        public string Name;
        public Script Script;

        public BehaviorFunction(uint romAddr, string bhvName, string name, Script script)
        {
            RomAddr = romAddr;
            BhvName = bhvName;
            Name = name;
            Script = script;
        }
    }
}
