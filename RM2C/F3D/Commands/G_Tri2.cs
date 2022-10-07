using System;
using System.Linq;

namespace RM2ExCoop.RM2C.F3DCommands
{
    internal class G_Tri2 : F3DCommand
    {
        readonly G_Tri1 tri1;
        readonly G_Tri1 tri2;

        public G_Tri2(G_Tri1 tri1_1, G_Tri1 tri1_2)
            : base(0xFF, "gsSP2Triangles")
        {
            tri1 = tri1_1;
            tri2 = tri1_2;
        }

        protected override void Decode(BitStream bin, string idPrefix)
        {
            throw new NotImplementedException();
        }

        protected override dynamic[] GetArgs()
        {
            return tri1.Args.Concat(tri2.Args).ToArray();
        }

        public override string ToString() => $"{Name}({tri1.ArgsToString()},{tri2.ArgsToString()}){Suffix}";
    }
}
