using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class GUIHelper
    {
        private const float INDENT_WIDTH = 15f;
        private static readonly float GUI_SPACE = EditorGUIUtility.standardVerticalSpacing;
        internal static readonly float propertyHeight = EditorGUIUtility.singleLineHeight;
        private static readonly GUIContent hideContent = new(" ");

        // 汎用的なFoldout
        private static bool Foldout(Rect position, SerializedProperty property, bool drawFoldout, GUIContent content = null)
        {
            var label = EditorGUI.BeginProperty(position, content ?? Localization.G(property), property);
            if(drawFoldout)
            {
                // Foldoutを描画する場合は左にずらして位置調整
                var rect = new Rect(position);
                if(EditorGUIUtility.hierarchyMode) rect.xMin -=  EditorStyles.foldout.padding.left - EditorStyles.label.padding.left;
                if(Event.current.type == EventType.Repaint) EditorStyles.foldoutHeader.Draw(rect, false, false, property.isExpanded, false);
                PropertyFoldout(position, property, label);
            }
            else
            {
                // Foldoutを描画しない場合は普通にラベルを表示
                EditorGUI.LabelField(position, label);
                property.isExpanded = true;
            }
            EditorGUI.EndProperty();
            return property.isExpanded;
        }

        // EditorGUILayout用
        private static bool Foldout(SerializedProperty prop, bool drawFoldout, GUIContent content = null)
        {
            return Foldout(EditorGUILayout.GetControlRect(), prop, drawFoldout, content);
        }

        // Foldoutの三角形の部分だけ
        internal static bool FoldoutOnly(Rect position, SerializedProperty property)
        {
            position.width = 12;
            position.height = propertyHeight;
            position.x -= 12;
            var label = EditorGUI.BeginProperty(position, GUIContent.none, property);
            position.x += 12;
            PropertyFoldout(position, property, label);
            EditorGUI.EndProperty();
            return property.isExpanded;
        }

        private static bool PropertyFoldout(Rect position, SerializedProperty property, GUIContent label)
        {
            var isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if(property.isExpanded != isExpanded)
            {
                // altキーが押されている場合は再帰的に開く
                if(Event.current.alt) SetExpandedRecurse(property, isExpanded);
                else property.isExpanded = isExpanded;
            }
            return isExpanded;
        }

        // 再帰的にFoldoutを開く
        private static void SetExpandedRecurse(SerializedProperty property, bool expanded)
        {
            using var iter = property.Copy();
            iter.isExpanded = expanded;
            int depth = iter.depth;
            bool visitChild = true;
            while(iter.NextVisible(visitChild) && iter.depth > depth)
            {
                visitChild = iter.propertyType != SerializedPropertyType.String;
                if(iter.hasVisibleChildren) iter.isExpanded = expanded;
            }
        }

        // 子を取得しつつFieldを表示
        internal static bool ChildField(Rect position, SerializedProperty property, string childName)
        {
            using var p = property.FPR(childName);
            return EditorGUI.PropertyField(position, p, Localization.G(p));
        }

        // ラベルなしでFieldを表示
        internal static bool ChildFieldOnly(Rect position, SerializedProperty property, string childName)
        {
            using var p = property.FPR(childName);
            return EditorGUI.PropertyField(position, p, GUIContent.none);
        }

        // プレースホルダ付きのTextField
        internal static void TextField(Rect position, GUIContent label, SerializedProperty property, string placeholder)
        {
            EditorGUI.PropertyField(position, property, label);
            if(string.IsNullOrEmpty(property.stringValue) && !string.IsNullOrEmpty(placeholder))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.LabelField(position, hideContent, new GUIContent(placeholder), placeholderStyle);
                EditorGUI.EndDisabledGroup();
            }
        }

        // プレースホルダ付きのObjectField
        internal static void ObjectField(Rect position, GUIContent label, SerializedProperty property, string placeholder)
        {
            EditorGUI.PropertyField(position, property, label);
            if(!property.objectReferenceValue && !string.IsNullOrEmpty(placeholder) && GUI.enabled)
            {
                EditorGUI.LabelField(position, hideContent, new GUIContent(placeholder), placeholderObjectStyle);
                GUIStyle buttonStyle = "ObjectFieldButton";
                Rect position2 = buttonStyle.margin.Remove(new Rect(position.xMax - 19f, position.y, 19f, position.height));
                EditorGUI.LabelField(position2, GUIContent.none, buttonStyle);
            }
        }

        // D&Dに対応するtypeを取得
        private static Dictionary<string,Type> subclassOfObject = typeof(Object).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Object))).ToDictionary(t => t.Name, t => t);
        private static Dictionary<string,Type> subclassOfComponent = typeof(Object).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Component))).ToDictionary(t => t.Name, t => t);
        internal static Rect AutoField(Rect position, SerializedProperty property, bool drawFoldout = true)
        {
            // 配列の描画
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                var prop = property.Copy();
                var arrayElementType = prop.arrayElementType;

                // 参照でない場合はD&D非対応
                if(!arrayElementType.StartsWith("PPtr<$")) return List(position, prop, drawFoldout);
                arrayElementType = arrayElementType.Replace("PPtr<$","").Replace(">","");

                // Componentである場合はD&D対応
                // D&DされたものがGameObjectである場合はGetComponent()
                if(subclassOfComponent.ContainsKey(arrayElementType))
                {
                    var type = subclassOfComponent[arrayElementType];
                    return DragAndDropList(position, prop, drawFoldout, null, null, o => {
                        if(o.GetType().Name == arrayElementType) return true;
                        if(o is GameObject g && g.GetComponent(type)) return true;
                        return false;
                    });
                }

                // Objectである場合は普通にD&D対応
                if(subclassOfObject.ContainsKey(arrayElementType))
                {
                    return DragAndDropList(position, prop, drawFoldout, null, null, o => o.GetType().Name == arrayElementType);
                }

                // Objectでない場合はD&D非対応
                return List(position, prop, drawFoldout);
            }

            // 配列以外の描画
            else
            {
                EditorGUI.PropertyField(position.SetHeight(property), property);
                return position.NewLine();
            }
        }

        // EditorGUILayout版
        internal static void AutoField(SerializedProperty property, bool drawFoldout = true)
        {
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                using var copy = property.Copy();
                List(copy, drawFoldout);
            }
            else
            {
                EditorGUILayout.PropertyField(property);
            }
        }

        internal static float GetAutoFieldHeight(SerializedProperty property, bool drawFoldout = true)
        {
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                using var copy = property.Copy();
                return GetListHeight(copy, drawFoldout) + GetSpaceHeight();
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property) + GetSpaceHeight();
            }
        }

        internal static float GetPropertyHeight(SerializedProperty property, string name)
        {
            using var prop = property.FindPropertyRelative(name);
            return EditorGUI.GetPropertyHeight(prop);
        }

        internal static float GetPropertyHeight(SerializedProperty property, int i)
        {
            using var prop = property.GetArrayElementAtIndex(i);
            return EditorGUI.GetPropertyHeight(prop);
        }

        // ラベルなし
        internal static bool FieldOnly(Rect position, SerializedProperty property)
        {
            return EditorGUI.PropertyField(position, property, GUIContent.none);
        }

        // D&D可能なリスト
        internal static Rect DragAndDropList<T>(Rect position, SerializedProperty property, bool drawFoldout, string childName, Action<SerializedProperty> initializeFunction, Action<SerializedProperty, Object> actionPerObject = null) where T : Component
        {
            return DragAndDropList(position, property, drawFoldout, childName, initializeFunction, o => (o is GameObject g) && g.GetComponent<T>(), actionPerObject);
        }

        internal static Rect DragAndDropList(Rect position, SerializedProperty property, bool drawFoldout, string childName, Action<SerializedProperty> initializeFunction, Action<SerializedProperty, Object> actionPerObject = null)
        {
            return DragAndDropList(position, property, drawFoldout, childName, initializeFunction, o => o is GameObject, actionPerObject);
        }

        internal static Rect DragAndDropList(Rect position, SerializedProperty property, bool drawFoldout, string childName, Action<SerializedProperty> initializeFunction, Func<Object, bool> selectFunc, Action<SerializedProperty, Object> actionPerObject = null)
        {
            var e = Event.current;
            int itemCount = 0;
            var items = new Object[]{};

            // D&D中のものの中から型が一致しているものだけ抽出
            if(DragAndDrop.objectReferences != null) items = DragAndDrop.objectReferences.Where(selectFunc).Where(o => o).ToArray();
            itemCount = items.Length;

            var rectDandD = new Rect(position)
            {
                // D&D中のものが1つの場合はラベルだけD&D対象、そうでない場合はリスト全体がD&D対象
                height = itemCount == 1 ? EditorGUIUtility.singleLineHeight : GetListHeight(property, drawFoldout) - EditorGUIUtility.singleLineHeight
            };
            bool isDragSingleObject = rectDandD.Contains(e.mousePosition) && itemCount == 1;
            bool isDragMultiObject = rectDandD.Contains(e.mousePosition) && itemCount > 1;

            // 複数オブジェクトをD&Dしている場合はリストをグレーアウトさせてD&D可能な旨のボックスを表示
            EditorGUI.BeginDisabledGroup(isDragMultiObject);
            position = List(position, property, drawFoldout, initializeFunction);
            EditorGUI.EndDisabledGroup();
            if(isDragMultiObject) DrawDropLect(rectDandD);

            if(isDragSingleObject || isDragMultiObject)
            {
                switch(e.type)
                {
                    case EventType.DragPerform:
                        DragAndDrop.AcceptDrag();

                        // 既存要素を取得
                        var exists = new List<Object>();
                        for(int i = 0; i < property.arraySize; i++)
                        {
                            using var obj = property.GetArrayElementAtIndex(i);
                            if(string.IsNullOrEmpty(childName))
                            {
                                if(obj.objectReferenceValue) exists.Add(obj.objectReferenceValue);
                            }
                            else
                            {
                                using var child = obj.FPR(childName);
                                if(child.objectReferenceValue) exists.Add(child.objectReferenceValue);
                            }
                        }

                        // 既存要素と重複するものは除外
                        var objectsToAdd = items.Where(o => o && !exists.Contains(o)).ToArray();
                        foreach(var o in objectsToAdd)
                        {
                            property.InsertArrayElementAtIndex(property.arraySize);
                            using var current = property.GetArrayElementAtIndex(property.arraySize - 1);
                            if(initializeFunction != null) initializeFunction.Invoke(current);
                            if(!string.IsNullOrEmpty(childName))
                            {
                                using var child = current.FPR(childName);
                                child.objectReferenceValue = o;
                            }
                            else current.objectReferenceValue = o;
                            actionPerObject?.Invoke(current, o);
                        }
                        e.Use();
                        break;
                    default:
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        break;
                }
            }

            return position;
        }

        // D&D可能な旨を表示するBox
        internal static void DrawDropLect(Rect position)
        {
            dropStyle.fontSize = (int)Mathf.Min(24, position.height);
            EditorGUI.LabelField(position, Localization.G("inspector.dragAndDrop"), dropStyle);
        }

        internal static GUIContent[] CreateContents(string[] labels)
        {
            return labels.Select(l => new GUIContent(l)).ToArray();
        }
    }
}
