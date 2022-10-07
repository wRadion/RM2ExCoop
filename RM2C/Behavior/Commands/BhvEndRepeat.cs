using System;

namespace RM2ExCoop.RM2C.BehaviorCommands
{
    internal class BhvEndRepeat : BhvNone
    {
        // Marks the end of a repeating loop.
        public BhvEndRepeat() : base(0x06, "END_REPEAT") { }
    }
}
