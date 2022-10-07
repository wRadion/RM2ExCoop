using System;

namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_SNoOp : F3DCommand
    {
        public G_SNoOp(byte code, string name) : base(code, name) { }

        protected override void Decode(BitStream bin, string idPrefix)
        {
        }

        protected override dynamic[] GetArgs() => Array.Empty<dynamic>();
    }
}
