using System;
using UnityEngine;

namespace jp.lilxyzw.materialmodifier.runtime
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
    internal class LILLocalizeAttribute : PropertyAttribute
    {
        public readonly string name;
        public LILLocalizeAttribute() => name = null;
        public LILLocalizeAttribute(string name) => this.name = name;
    }
}
