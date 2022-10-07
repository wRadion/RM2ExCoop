using System;

namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_EndDL : F3DCommand
    {
        public G_EndDL(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
        }

        protected override dynamic[] GetArgs() => Array.Empty<dynamic>();
    }
}
