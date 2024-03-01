using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

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

        internal static void DoAllElements(this SerializedProperty property, Action<SerializedProperty,int> function)
        {
            if(property.arraySize == 0) return;
            int i = 0;
            var prop = property.GetArrayElementAtIndex(0);
            var end = property.GetEndProperty();
            function.Invoke(prop,i);
            while(prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, end))
            {
                i++;
                function.Invoke(prop,i);
            }
        }

        #if LIL_AVATAR_MODIFIER
        [MenuItem("Tools/lilycalInventory/Migrate From lilAvatarModifier")]
        private static void Test()
        {
            if(!Selection.activeGameObject) return;
            const string NAME_SPACE = "jp.lilxyzw.avatarmodifier.runtime";
            var assembly = Assembly.Load(NAME_SPACE);
            ReplaceScript<AutoDresser>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.AutoDresser");
            ReplaceScript<CostumeChanger>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.CostumeChanger");
            ReplaceScript<ItemToggler>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.ItemToggler");
            ReplaceScript<MaterialModifier>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.MaterialModifier");
            ReplaceScript<MaterialOptimizer>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.MaterialOptimizer");
            ReplaceScript<MenuFolder>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.MenuFolder");
            ReplaceScript<Prop>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.Prop");
            ReplaceScript<SmoothChanger>(Selection.activeGameObject, assembly, $"{NAME_SPACE}.SmoothChanger");
        }

        private static void ReplaceScript<T>(GameObject gameObject, Assembly assembly, string classname) where T : MonoBehaviour
        {
            var tempobj = new GameObject();
            var components = gameObject.GetComponentsInChildren(assembly.GetType(classname), true);
            var monoScript = MonoScript.FromMonoBehaviour(tempobj.AddComponent<T>());
            foreach(var component in components)
            {
                var so = new SerializedObject(component);
                so.Update();
                so.FindProperty("m_Script").objectReferenceValue = monoScript;
                so.ApplyModifiedProperties();
            }
            Object.DestroyImmediate(tempobj);
        }
        #endif
    }
}
