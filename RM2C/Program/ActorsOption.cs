namespace RM2ExCoop.RM2C
{
    public enum ActorsOptionType
    {
        NONE, ALL, OLD, NEW, GROUP, GROUPS
    }

    internal class ActorsOption
    {
        public ActorsOptionType Type;
        public string[] Groups;

        public string Group => Groups[0];

        public ActorsOption(ActorsOptionType type = ActorsOptionType.NONE, params string[] groups)
        {
            Type = type;
            Groups = groups;
        }   
    }
}
