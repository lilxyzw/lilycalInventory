namespace jp.lilxyzw.lilycalinventory
{
    internal class InternalParameter
    {
        internal string name;
        internal float defaultValue;
        internal bool isLocalOnly;
        internal bool isSave;
        internal InternalParameterType type;

        internal InternalParameter(string name, float defaultValue, bool isLocalOnly, bool isSave, InternalParameterType type)
        {
            this.name = name;
            this.defaultValue = defaultValue;
            this.isLocalOnly = isLocalOnly;
            this.isSave = isSave;
            this.type = type;
        }
    }

    internal enum InternalParameterType
    {
        Bool,
        Int,
        Float
    }
}
