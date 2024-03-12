using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class GUIHelper
    {
        private static readonly Dictionary<string, ReorderableList> reorderableLists = new Dictionary<string, ReorderableList>();
        internal static void ResetList() => reorderableLists.Clear();

        internal static Rect List(Rect position, SerializedProperty property, bool drawFoldout, Action<SerializedProperty> initializeFunction = null)
        {
            return InternalList(position, property, drawFoldout, initializeFunction);
        }

        internal static Rect List(Rect position, SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
        {
            return InternalList(position, property, true, initializeFunction);
        }

        private static Rect InternalList(Rect position, SerializedProperty property, bool drawFoldout, Action<SerializedProperty> initializeFunction)
        {
            if(!Foldout(position.SingleLine(), property, drawFoldout)) return position.NewLine();
            position.NewLine();
            var reorderableList = TryGetReorderableList(property, initializeFunction);
            position.height = reorderableList.GetHeight();
            reorderableList.DoList(position);
            position.y = position.yMax;
            return position;
        }

        private static void List(SerializedProperty property, bool drawFoldout, Action<SerializedProperty> initializeFunction = null)
        {
            InternalList(property, drawFoldout, initializeFunction);
        }

        private static void List(SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
        {
            InternalList(property, true, initializeFunction);
        }

        private static void InternalList(SerializedProperty property, bool drawFoldout, Action<SerializedProperty> initializeFunction)
        {
            if(Foldout(property, drawFoldout))
                TryGetReorderableList(property, initializeFunction).DoLayoutList();
        }

        private static ReorderableList TryGetReorderableList(SerializedProperty property, Action<SerializedProperty> initializeFunction)
        {
            var name = property.GetUniqueName();
            if(!reorderableLists.ContainsKey(name))
                reorderableLists[name] = CreateReorderableList(property, initializeFunction);
            return reorderableLists[name];
        }

        internal static float GetListHeight(SerializedProperty property, bool drawFoldout = true)
        {
            if(drawFoldout && !property.isExpanded) return propertyHeight;
            var name = property.GetUniqueName();
            if(!reorderableLists.ContainsKey(name)) return EditorGUI.GetPropertyHeight(property);
            return reorderableLists[name].GetHeight() + propertyHeight;
        }

        private static ReorderableList CreateReorderableList(SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
        {
            Rect headerRect = default(Rect);
            var list = new ReorderableList(property.serializedObject, property)
            {
                draggable = true,
                headerHeight = 0,
                //footerHeight = 0,
                #if UNITY_2021_1_OR_NEWER
                multiSelect = true,
                #endif
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)),
                onAddCallback = list => property.ResizeArray(property.arraySize + 1, initializeFunction),
                drawHeaderCallback = rect => headerRect = rect,
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
            list.drawFooterCallback = rect =>
            {
                headerRect.height = EditorGUIUtility.singleLineHeight;
                headerRect.y -= headerRect.height + EditorGUIUtility.standardVerticalSpacing;
                DrawFooter(headerRect, list);
            };
            return list;
        }

        private static string GetUniqueName(this SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}.{property.propertyPath}";
        }

        private static MethodInfo InvalidateCacheRecursive;
        private static FieldInfo m_scheduleRemove;
        private static bool isInitialized = false;
        private static void DrawFooter(Rect rect, ReorderableList list)
        {
            if(!isInitialized)
            {
                isInitialized = true;
                InvalidateCacheRecursive = typeof(ReorderableList).GetMethod("InvalidateCacheRecursive", BindingFlags.Instance | BindingFlags.NonPublic);
                m_scheduleRemove = typeof(ReorderableList).GetField("m_scheduleRemove", BindingFlags.Instance | BindingFlags.NonPublic);

                if(InvalidateCacheRecursive == null) Debug.LogError("InvalidateCacheRecursive == null");
                if(m_scheduleRemove == null) Debug.LogError("m_scheduleRemove == null");
            }

            bool isOverMaxMultiEditLimit = list.serializedProperty != null && list.serializedProperty.minArraySize > list.serializedProperty.serializedObject.maxArraySizeForMultiEditing && list.serializedProperty.serializedObject.isEditingMultipleObjects;

            var rectNum = new Rect(rect.xMax - EditorGUIUtility.fieldWidth + EditorGUIUtility.standardVerticalSpacing * 3, rect.y, EditorGUIUtility.fieldWidth, rect.height);
            var rectRem = new Rect(rectNum.x - 40 - EditorGUIUtility.standardVerticalSpacing, rect.y, 40, rect.height);
            var rectAdd = new Rect(rectRem.x - 40 - EditorGUIUtility.standardVerticalSpacing, rect.y, 40, rect.height);
            var rectBack = new Rect(rectAdd.x, rect.y, rect.xMax - rectAdd.x, EditorGUIUtility.singleLineHeight);
            EditorGUI.DrawRect(rectBack, EditorGUIUtility.isProSkin ? new Color(0.219f,0.219f,0.219f,1) : new Color(0.784f,0.784f,0.784f,1));

            EditorGUI.BeginChangeCheck();
            var size = EditorGUI.IntField(rectNum, list.serializedProperty.arraySize);
            if(EditorGUI.EndChangeCheck()) list.serializedProperty.arraySize = size;

            if(list.displayAdd)
            {
                bool cantAdd = list.onCanAddCallback != null && !list.onCanAddCallback(list) || isOverMaxMultiEditLimit;
                using(new EditorGUI.DisabledScope(cantAdd))
                {
                    EditorGUI.DrawRect(rectAdd, new Color(0,0,0,0.1f));
                    if(GUI.Button(rectAdd, Localization.G("inspector.add"), ReorderableList.defaultBehaviours.preButton))
                    {
                        if(list.onAddDropdownCallback != null) list.onAddDropdownCallback(rectAdd, list);
                        else if(list.onAddCallback != null) list.onAddCallback(list);
                        else ReorderableList.defaultBehaviours.DoAddButton(list);

                        list.onChangedCallback?.Invoke(list);
                        InvalidateCacheRecursive?.Invoke(list, null);
                    }
                }
            }
            if(list.displayRemove)
            {
                bool cantRemove = list.index < 0 || list.index >= list.count || (list.onCanRemoveCallback != null && !list.onCanRemoveCallback(list)) || isOverMaxMultiEditLimit;
                using(new EditorGUI.DisabledScope(cantRemove))
                {
                    EditorGUI.DrawRect(rectRem, new Color(0,0,0,0.1f));
                    if(GUI.Button(rectRem, Localization.G("inspector.delete"), ReorderableList.defaultBehaviours.preButton) || GUI.enabled && (bool)m_scheduleRemove?.GetValue(list))
                    {
                        if(list.onRemoveCallback == null) ReorderableList.defaultBehaviours.DoRemoveButton(list);
                        else list.onRemoveCallback(list);

                        list.onChangedCallback?.Invoke(list);
                        InvalidateCacheRecursive?.Invoke(list, null);
                        GUI.changed = true;
                    }
                }
            }

            m_scheduleRemove?.SetValue(list, false);
        }
    }
}
