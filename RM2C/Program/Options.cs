using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal struct Options
    {
        public LevelsOption Levels;
        public ActorsOption Actors;
        public ObjectsOption Objects;
        public bool Editor = false;
        public bool WaterOnly = false;
        public bool ObjectOnly = false;
        public bool MusicOnly = false;
        public uint MusicExtend = 0;
        public bool Text = false;
        public bool Misc = false;
        public bool Segment2 = false;
        public bool Skyboxes = false;
        public bool Sound = false;
        public bool Inherit = false;

        public bool OnlySkip => new bool[] { WaterOnly, ObjectOnly, MusicOnly }.Any(b => b);

        public Options()
        {
            Levels = new();
            Actors = new();
            Objects = new();
        }
    }
}
