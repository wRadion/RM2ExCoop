namespace RM2ExCoop.RM2C
{
    internal class BehaviorPointer
    {
        public uint Start;
        public Script Script;
        public string BhvName;

        public BehaviorPointer(uint start, Script script, string bhvName)
        {
            Start = start;
            Script = script;
            BhvName = bhvName;
        }
    }
}
