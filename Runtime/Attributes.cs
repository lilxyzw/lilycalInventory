using System;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class MenuFolderOverrideAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class OneLineVectorAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class NoLabelAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class MenuNameAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class CostumeNameAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class LILBoxAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class LILLocalizeAttribute : PropertyAttribute
    {
        public readonly string name;
        public LILLocalizeAttribute() => name = null;
        public LILLocalizeAttribute(string name) => this.name = name;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class LILLocalizeHeaderAttribute : PropertyAttribute
    {
        public readonly string name;
        public LILLocalizeHeaderAttribute(string name, int order = 0)
        {
            this.name = name;
            this.order = order;
        }
    }
}
