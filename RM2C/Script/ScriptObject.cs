using System.Collections.Generic;

namespace RM2ExCoop.RM2C
{
    internal class ScriptObject
    {
        public uint RamAddr;
        public uint RomAddr;
        public List<ScriptModel> Models;
        public Script Script;

        public ScriptObject(uint ramAddr, uint romAddr, Script script)
        {
            RamAddr = ramAddr;
            RomAddr = romAddr;
            Models = new();
            Script = script;
        }
    }
}
