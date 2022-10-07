namespace RM2ExCoop.RM2C
{
    public enum LevelsOptionType
    {
        ALL, LIST
    }

    internal class LevelsOption
    {
        public LevelsOptionType Type;
        public int[] Levels;

        public LevelsOption(params int[] levels)
        {
            Type = levels.Length > 0 ? LevelsOptionType.LIST : LevelsOptionType.ALL;
            Levels = levels;
        }
    }
}
