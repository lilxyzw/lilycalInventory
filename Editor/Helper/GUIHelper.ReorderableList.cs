using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
{
    internal static partial class GUIHelper
    {
        private static readonly Dictionary<string, ReorderableList> reorderableLists = new Dictionary<string, ReorderableList>();
        internal static void ResetList() => reorderableLists.Clear();

        internal static Rect List(Rect position, SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
        {
            if(!Foldout(position.SingleLine(), property)) return position.NewLine();
            position.NewLine();
            var reorderableList = TryGetReorderableList(property, initializeFunction);
            position.height = reorderableList.GetHeight();
            reorderableList.DoList(position);
            position.y = position.yMax;
            return position;
        }

        private static void List(SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
        {
            if(Foldout(property))
                TryGetReorderableList(property, initializeFunction).DoLayoutList();
        }

        private static ReorderableList TryGetReorderableList(SerializedProperty property, Action<SerializedProperty> initializeFunction)
        {
            var name = property.GetUniqueName();
            if(!reorderableLists.ContainsKey(name))
                reorderableLists[name] = CreateReorderableList(property, initializeFunction);
            return reorderableLists[name];
        }

        internal static float GetListHeight(SerializedProperty property)
        {
            if(!property.isExpanded) return propertyHeight;
            var name = property.GetUniqueName();
            if(!reorderableLists.ContainsKey(name)) return EditorGUI.GetPropertyHeight(property);
            return reorderableLists[name].GetHeight() + propertyHeight;
        }

        private static ReorderableList CreateReorderableList(SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
        {
            return new ReorderableList(property.serializedObject, property)
                {
                    draggable = true,
                    headerHeight = 0,
                    #if UNITY_2021_1_OR_NEWER
                    multiSelect = true,
                    #endif
                    elementHeightCallback = index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)),
                    onAddCallback = list => property.ResizeArray(property.arraySize + 1, initializeFunction),
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.x += 8;
                        rect.width -= 8;
                        var elementProperty = property.GetArrayElementAtIndex(index);
                        rect.y += GUI_SPACE * 0.5f;
                        rect.height -= GUI_SPACE;
                        EditorGUI.PropertyField(rect, elementProperty);
                    }
                };
        }

        private static string GetUniqueName(this SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}.{property.propertyPath}";
        }
    }
}
