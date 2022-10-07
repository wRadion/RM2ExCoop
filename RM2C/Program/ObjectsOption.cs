namespace RM2ExCoop.RM2C
{
    public enum ObjectsOptionType
    {
        NONE, ALL, NEW, BEHAVIORS, BEHAVIOR
    }

    internal class ObjectsOption
    {
        public ObjectsOptionType Type;
        public string[] Behaviors;

        public string Behavior => Behaviors[0];

        public ObjectsOption(ObjectsOptionType type = ObjectsOptionType.NONE, params string[] behaviors)
        {
            Type = type;
            Behaviors = behaviors;
        }
    }
}
