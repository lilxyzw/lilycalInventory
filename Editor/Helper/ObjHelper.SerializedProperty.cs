using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class ObjHelper
    {
        // FindPropertyRelativeが長いわりに使い所がかなり多いので省略できるように
        internal static SerializedProperty FPR(this SerializedProperty property, string name)
        {
            return property.FindPropertyRelative(name);
        }

        // 配列リサイズ時に所定の値で埋められるように
        internal static void ResizeArray(this SerializedProperty prop, int size, Action<SerializedProperty> initializeFunction = null)
        {
            var arraySize = prop.arraySize;
            if(arraySize == size) return;
            prop.arraySize = size;
            if(initializeFunction != null && size > arraySize)
                for(int i = arraySize; i < size; i++)
                {
                    using var element = prop.GetArrayElementAtIndex(i);
                    initializeFunction.Invoke(element);
                }
        }

        // 配列から全オブジェクト名を取得
        internal static string[] GetAllObjectNames(this SerializedProperty property)
        {
            if(property.arraySize == 0) return new[]{"Empty"};
            var names = new List<string>();
            for(int i = 0; i < property.arraySize; i++)
                names.Add(property.GetObjectInProperty(i).TryGetName());
            return names.ToArray();
        }

        // 配列の全要素に対して所定の処理を実行
        internal static void DoAllElements(this SerializedProperty property, Action<SerializedProperty,int> function)
        {
            if(property.arraySize == 0) return;
            int i = 0;
            using var prop = property.GetArrayElementAtIndex(0);
            using var end = property.GetEndProperty();
            {
                function.Invoke(prop,i);
                while(prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, end))
                {
                    i++;
                    function.Invoke(prop,i);
                }
            }
        }

        // 配列要素内のプロパティを取得
        internal static SerializedProperty GetPropertyInArrayElement(this SerializedProperty serializedProperty, int i, string name)
        {
            using var element = serializedProperty.GetArrayElementAtIndex(i);
            return element.FPR(name);
        }

        // オブジェクトを取得
        internal static Object GetObjectInProperty(this SerializedObject serializedObject, string name)
        {
            using var prop = serializedObject.FindProperty(name);
            return prop.objectReferenceValue;
        }

        internal static Object GetObjectInProperty(this SerializedProperty serializedProperty, string name)
        {
            using var prop = serializedProperty.FPR(name);
            return prop.objectReferenceValue;
        }

        internal static Object GetObjectInProperty(this SerializedProperty serializedProperty, int i)
        {
            using var prop = serializedProperty.GetArrayElementAtIndex(i);
            return prop.objectReferenceValue;
        }

        internal static Object GetObjectInProperty(this SerializedProperty serializedProperty, int i, string name)
        {
            using var prop = serializedProperty.GetPropertyInArrayElement(i, name);
            return prop.objectReferenceValue;
        }

        // stringValueを取得
        internal static string GetStringInProperty(this SerializedObject serializedObject, string name)
        {
            using var prop = serializedObject.FindProperty(name);
            return prop.stringValue;
        }

        internal static string GetStringInProperty(this SerializedProperty serializedProperty, string name)
        {
            using var prop = serializedProperty.FPR(name);
            return prop.stringValue;
        }

        internal static string GetStringInProperty(this SerializedProperty serializedProperty, int i)
        {
            using var prop = serializedProperty.GetArrayElementAtIndex(i);
            return prop.stringValue;
        }

        internal static string GetStringInProperty(this SerializedProperty serializedProperty, int i, string name)
        {
            using var prop = serializedProperty.GetPropertyInArrayElement(i, name);
            return prop.stringValue;
        }
    }
}
