using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class GUIHelper
    {
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
            // Foldoutの表示
            if(!Foldout(position.SingleLine(), property, drawFoldout)) return position.NewLine();
            position.NewLine();

            // Listの表示
            var reorderableList = PropertyHandlerWrap.GetOrSet(property, initializeFunction);
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
                PropertyHandlerWrap.GetOrSet(property, initializeFunction).DoLayoutList();
        }

        internal static float GetListHeight(SerializedProperty property, bool drawFoldout = true)
        {
            if(drawFoldout && !property.isExpanded) return propertyHeight;
            var list = PropertyHandlerWrap.GetOrSet(property);
            if(list == null) return EditorGUI.GetPropertyHeight(property);
            return list.GetHeight() + propertyHeight;
        }

        internal static float GetListHeight(SerializedProperty parent, string propertyName, bool drawFoldout = true)
        {
            using var property = parent.FPR(propertyName);
            if(drawFoldout && !property.isExpanded) return propertyHeight;
            var list = PropertyHandlerWrap.GetOrSet(property);
            if(list == null) return EditorGUI.GetPropertyHeight(property);
            return list.GetHeight() + propertyHeight;
        }

        private static ReorderableList CreateReorderableList(SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
        {
            Rect headerRect = default;
            var list = new ReorderableList(property.serializedObject, property.Copy(), true, false, true, true)
            {
                draggable = true,
                headerHeight = 0,
                //footerHeight = 0, // みやすさのためにあえて余白を残す
                multiSelect = true,
                drawHeaderCallback = rect => headerRect = rect
            };
            list.elementHeightCallback = index => GetPropertyHeight(list.serializedProperty, index);
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.x += 8;
                rect.width -= 8;
                rect.y += GUI_SPACE * 0.5f;
                rect.height -= GUI_SPACE;
                EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index));
            };
            if(initializeFunction != null)
                list.onAddCallback = _ => list.serializedProperty.ResizeArray(list.serializedProperty.arraySize + 1, initializeFunction);

            // フッターはヘッダーの位置にずらして操作しやすく
            // ついでに表示もカスタマイズ
            list.drawFooterCallback = rect =>
            {
                headerRect.height = EditorGUIUtility.singleLineHeight;
                headerRect.y -= headerRect.height + EditorGUIUtility.standardVerticalSpacing;
                DrawFooter(headerRect, list);
            };
            return list;
        }

        private static MethodInfo InvalidateCacheRecursive;
        private static FieldInfo m_scheduleRemove;
        private static bool isInitialized = false;
        private static void DrawFooter(Rect rect, ReorderableList list)
        {
            // どうしようもないのでReflectionを使用
            if(!isInitialized)
            {
                isInitialized = true;
                InvalidateCacheRecursive = typeof(ReorderableList).GetMethod("InvalidateCacheRecursive", BindingFlags.Instance | BindingFlags.NonPublic);
                m_scheduleRemove = typeof(ReorderableList).GetField("m_scheduleRemove", BindingFlags.Instance | BindingFlags.NonPublic);

                if(InvalidateCacheRecursive == null) Debug.LogError("InvalidateCacheRecursive == null");
                if(m_scheduleRemove == null) Debug.LogError("m_scheduleRemove == null");
            }

            bool isOverMaxMultiEditLimit = list.serializedProperty != null &&
                list.serializedProperty.minArraySize > list.serializedProperty.serializedObject.maxArraySizeForMultiEditing &&
                list.serializedProperty.serializedObject.isEditingMultipleObjects;

            var rectNum = new Rect(rect.xMax - EditorGUIUtility.fieldWidth + EditorGUIUtility.standardVerticalSpacing * 3, rect.y, EditorGUIUtility.fieldWidth, rect.height);
            var rectRem = new Rect(rectNum.x - 40 - EditorGUIUtility.standardVerticalSpacing, rect.y, 40, rect.height);
            var rectAdd = new Rect(rectRem.x - 40 - EditorGUIUtility.standardVerticalSpacing, rect.y, 40, rect.height);
            var rectBack = new Rect(rectAdd.x, rect.y, rect.xMax - rectAdd.x, EditorGUIUtility.singleLineHeight);

            // Foldoutのラベルと重なることを防ぐために上からRectを描画
            EditorGUI.DrawRect(rectBack, EditorGUIUtility.isProSkin ? new Color(0.219f,0.219f,0.219f,1) : new Color(0.784f,0.784f,0.784f,1));

            // 配列の要素数を表示
            EditorGUI.BeginChangeCheck();
            var size = EditorGUI.IntField(rectNum, list.serializedProperty.arraySize);
            if(EditorGUI.EndChangeCheck()) list.serializedProperty.arraySize = size;

            // 追加ボタン、削除ボタンの再実装
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

        private class ReorderableListWrapper
        {
            private static Type TYPE = typeof(ReorderableList).Assembly.GetType("UnityEditorInternal.ReorderableListWrapper");
            private static ConstructorInfo CI = TYPE.GetConstructor(new Type[]{typeof(SerializedProperty), typeof(GUIContent), typeof(bool)});
            private static MethodInfo MI_GetPropertyIdentifier = TYPE.GetMethod("GetPropertyIdentifier", BindingFlags.Public | BindingFlags.Static);
            private static PropertyInfo PI_Property = TYPE.GetProperty("Property", BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo FI_m_ReorderableList = TYPE.GetField("m_ReorderableList", BindingFlags.NonPublic | BindingFlags.Instance);
            internal object instance;

            internal static string GetPropertyIdentifier(SerializedProperty serializedProperty)
                => MI_GetPropertyIdentifier.Invoke(null, new object[]{serializedProperty}) as string;

            internal ReorderableListWrapper(SerializedProperty property, GUIContent label, bool reorderable = true)
                => instance = CI.Invoke(new object[]{property, label, reorderable});

            internal ReorderableListWrapper(object instance)
                => this.instance = instance;

            internal SerializedProperty Property
            {
                get => PI_Property.GetValue(instance) as SerializedProperty;
                set => PI_Property.SetValue(instance, value);
            }

            private ReorderableList m_ReorderableListBuf;
            internal ReorderableList m_ReorderableList
            {
                get => m_ReorderableListBuf != null ? m_ReorderableListBuf : m_ReorderableListBuf = FI_m_ReorderableList.GetValue(instance) as ReorderableList;
                set => FI_m_ReorderableList.SetValue(instance, m_ReorderableListBuf = value);
            }
        }

        private class PropertyHandlerWrap
        {
            private static Type TYPE = typeof(Editor).Assembly.GetType("UnityEditor.PropertyHandler");
            private static FieldInfo FI_s_reorderableLists = TYPE.GetField("s_reorderableLists", BindingFlags.NonPublic | BindingFlags.Static);
            private static System.Collections.IDictionary s_reorderableLists;
            internal static ReorderableList GetOrSet(SerializedProperty property, Action<SerializedProperty> initializeFunction = null)
            {
                s_reorderableLists ??= FI_s_reorderableLists.GetValue(null) as System.Collections.IDictionary;

                var name = ReorderableListWrapper.GetPropertyIdentifier(property);
                ReorderableListWrapper wrapper;
                if(s_reorderableLists.Contains(name))
                {
                    wrapper = new(s_reorderableLists[name]);
                }
                else
                {
                    wrapper = new(property, GUIContent.none, true);
                    wrapper.m_ReorderableList = CreateReorderableList(property, initializeFunction);
                    s_reorderableLists[name] = wrapper.instance;
                }
                wrapper.Property = property.Copy();
                var reorderableList = wrapper.m_ReorderableList;
                if(initializeFunction != null && reorderableList.onAddCallback == null)
                    reorderableList.onAddCallback = _ => reorderableList.serializedProperty.ResizeArray(reorderableList.serializedProperty.arraySize + 1, initializeFunction);

                return reorderableList;
            }
        }
    }
}
