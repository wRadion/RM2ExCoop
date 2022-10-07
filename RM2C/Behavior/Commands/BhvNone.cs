using System;

namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal abstract class BhvNone : BehaviorCommand
    {
        protected BhvNone(int msb, string name) : base(msb, name, BehaviorParamType.NONE) { }

        public override dynamic[] GetArgs(BitStream bin)
        {
            _ = bin.ReadUInt32();

            return Array.Empty<dynamic>();
        }
    }
}
