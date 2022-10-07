using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal struct Level
    {
        public int Id;
        public string Name;
        public string LevelName;
        public Area?[] Areas;

        public Level(int id)
        {
            Id = id;
            Name = Data.Num2Name[id];
            LevelName = "LEVEL_" + (Name == "castle_inside" ? "castle" : Name).ToUpper();
            Areas = new Area?[8];
        }

        public Area[] GetAreas() => Areas.Where(a => a != null).Cast<Area>().ToArray();
    }
}
