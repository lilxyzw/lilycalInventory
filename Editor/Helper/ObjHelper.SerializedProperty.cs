using System;
using System.Collections.Generic;
using UnityEditor;

namespace jp.lilxyzw.materialmodifier
{
    internal static partial class ObjHelper
    {
        internal static SerializedProperty FPR(this SerializedProperty property, string name)
        {
            return property.FindPropertyRelative(name);
        }

        internal static void ResizeArray(this SerializedProperty prop, int size, Action<SerializedProperty> initializeFunction = null)
        {
            var arraySize = prop.arraySize;
            if(arraySize == size) return;
            prop.arraySize = size;
            if(initializeFunction != null && size > arraySize)
                for(int i = arraySize; i < size; i++)
                    initializeFunction.Invoke(prop.GetArrayElementAtIndex(i));
        }

        internal static string[] GetAllObjectNames(this SerializedProperty property)
        {
            if(property.arraySize == 0) return new[]{"Empty"};
            var names = new List<string>();
            for(int i = 0; i < property.arraySize; i++)
                names.Add(property.GetArrayElementAtIndex(i).objectReferenceValue.TryGetName());
            return names.ToArray();
        }
    }
}
