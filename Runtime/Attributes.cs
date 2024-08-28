using System;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
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
    internal class FrameAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class LILBoxAttribute : PropertyAttribute {}

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class DefaultValueAttribute : PropertyAttribute
    {
        public readonly string array;
        public DefaultValueAttribute() => array = null;
        public DefaultValueAttribute(string array) => this.array = array;
    }

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

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal class LILDisableWhenAttribute : PropertyAttribute
    {
        public readonly string propertyPath;
        public readonly bool boolValue;
        public readonly int intValue;
        public readonly float floatValue;
        public LILDisableWhenAttribute(string propertyPath, bool boolValue)
        {
            this.propertyPath = propertyPath;
            this.boolValue = boolValue;
        }
        public LILDisableWhenAttribute(string propertyPath, int intValue)
        {
            this.propertyPath = propertyPath;
            this.intValue = intValue;
        }
        public LILDisableWhenAttribute(string propertyPath, float floatValue)
        {
            this.propertyPath = propertyPath;
            this.floatValue = floatValue;
        }
    }
}
