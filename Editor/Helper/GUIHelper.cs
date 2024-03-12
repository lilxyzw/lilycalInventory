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
        private static readonly GUIContent hideContent = new GUIContent(" ");
        private static GUIStyle placeholderStyle
        {
            get
            {
                if(m_PlaceholderStyle == null) InitializeGUI();
                return m_PlaceholderStyle;
            }
        }
        private static GUIStyle placeholderObjectStyle
        {
            get
            {
                if(m_PlaceholderObjectStyle == null) InitializeGUI();
                return m_PlaceholderObjectStyle;
            }
        }
        private static GUIStyle dropStyle
        {
            get
            {
                if(m_DropStyle == null) InitializeGUI();
                return m_DropStyle;
            }
        }
        private static GUIStyle m_PlaceholderStyle;
        private static GUIStyle m_PlaceholderObjectStyle;
        private static GUIStyle m_DropStyle;
        internal static readonly float propertyHeight = EditorGUIUtility.singleLineHeight;

        private static void InitializeGUI()
        {
            m_PlaceholderStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Italic,
                padding = EditorStyles.textField.padding
            };
            m_PlaceholderObjectStyle = new GUIStyle(EditorStyles.objectField)
            {
                fontStyle = FontStyle.Italic,
                padding = EditorStyles.textField.padding
            };
            m_DropStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter
            };
            var col = EditorStyles.objectField.normal.textColor;
            SetColors(m_PlaceholderObjectStyle, new Color(col.r, col.g, col.b, 0.5f));
        }

        private static bool Foldout(Rect position, SerializedProperty property, bool drawFoldout, GUIContent content = null)
        {
            var label = EditorGUI.BeginProperty(position, content ?? Localization.G(property), property);
            if(drawFoldout)
            {
                var rect = new Rect(position);
                if(EditorGUIUtility.hierarchyMode) rect.xMin -=  EditorStyles.foldout.padding.left - EditorStyles.label.padding.left;
                if(Event.current.type == EventType.Repaint) EditorStyles.foldoutHeader.Draw(rect, false, false, property.isExpanded, false);
                PropertyFoldout(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label);
                property.isExpanded = true;
            }
            EditorGUI.EndProperty();
            return property.isExpanded;
        }

        private static bool Foldout(SerializedProperty prop, bool drawFoldout, GUIContent content = null)
        {
            return Foldout(EditorGUILayout.GetControlRect(), prop, drawFoldout, content);
        }

        // TODO
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
            EditorGUI.BeginChangeCheck();
            var isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if(property.isExpanded != isExpanded)
            {
                if(Event.current.alt) SetExpandedRecurse(property, isExpanded);
                else property.isExpanded = isExpanded;
            }
            return isExpanded;
        }

        private static void SetExpandedRecurse(SerializedProperty property, bool expanded)
        {
            SerializedProperty iter = property.Copy();
            iter.isExpanded = expanded;
            int depth = iter.depth;
            bool visitChild = true;
            while(iter.NextVisible(visitChild) && iter.depth > depth)
            {
                visitChild = iter.propertyType != SerializedPropertyType.String;
                if(iter.hasVisibleChildren) iter.isExpanded = expanded;
            }
        }

        // TODO
        internal static bool ChildField(Rect position, SerializedProperty property, string childName)
        {
            var p = property.FPR(childName);
            return EditorGUI.PropertyField(position, p, Localization.G(p));
        }

        internal static bool ChildFieldOnly(Rect position, SerializedProperty property, string childName)
        {
            return EditorGUI.PropertyField(position, property.FPR(childName), GUIContent.none);
        }

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

        private static Dictionary<string,Type> subclassOfObject = typeof(Object).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Object))).ToDictionary(t => t.Name, t => t);
        private static Dictionary<string,Type> subclassOfComponent = typeof(Object).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Component))).ToDictionary(t => t.Name, t => t);
        internal static Rect AutoField(Rect position, SerializedProperty property, bool drawFoldout = true)
        {
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                var prop = property.Copy();
                var arrayElementType = prop.arrayElementType;
                if(!arrayElementType.StartsWith("PPtr<$")) List(position, prop, drawFoldout);

                arrayElementType = arrayElementType.Replace("PPtr<$","").Replace(">","");
                if(subclassOfComponent.ContainsKey(arrayElementType))
                {
                    var type = subclassOfComponent[arrayElementType];
                    return DragAndDropList(position, prop, drawFoldout, null, null, o => {
                        if(o.GetType().Name == arrayElementType) return true;
                        if(o is GameObject g && g.GetComponent(type)) return true;
                        return false;
                    });
                }
                if(subclassOfObject.ContainsKey(arrayElementType))
                {
                    return DragAndDropList(position, prop, drawFoldout, null, null, o => o.GetType().Name == arrayElementType);
                }

                return List(position, prop, drawFoldout);
            }
            else
            {
                EditorGUI.PropertyField(position.SetHeight(property), property);
                return position.NewLine();
            }
        }

        internal static void AutoField(SerializedProperty property, bool drawFoldout = true)
        {
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                List(property.Copy(), drawFoldout);
            }
            else
            {
                EditorGUILayout.PropertyField(property);
            }
        }

        internal static float GetAutoFieldHeight(SerializedProperty property)
        {
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                return GetListHeight(property.Copy()) + GetSpaceHeight();
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property) + GetSpaceHeight();
            }
        }

        internal static bool FieldOnly(Rect position, SerializedProperty property)
        {
            return EditorGUI.PropertyField(position, property, GUIContent.none);
        }

        internal static Rect DragAndDropList<T>(Rect position, SerializedProperty property, bool drawFoldout, string childName, Action<SerializedProperty> initializeFunction) where T : Component
        {
            return DragAndDropList(position, property, drawFoldout, childName, initializeFunction, o => (o is GameObject g) && g.GetComponent<T>());
        }

        internal static Rect DragAndDropList(Rect position, SerializedProperty property, bool drawFoldout, string childName, Action<SerializedProperty> initializeFunction)
        {
            return DragAndDropList(position, property, drawFoldout, childName, initializeFunction, o => o is GameObject);
        }

        internal static Rect DragAndDropList(Rect position, SerializedProperty property, bool drawFoldout, string childName, Action<SerializedProperty> initializeFunction, Func<Object, bool> selectFunc)
        {
            var e = Event.current;
            int itemCount = 0;
            var items = new Object[]{};
            if(DragAndDrop.objectReferences != null) items = DragAndDrop.objectReferences.Where(selectFunc).Where(o => o).ToArray();
            itemCount = items.Length;

            var rectDandD = new Rect(position)
            {
                height = itemCount == 1 ? EditorGUIUtility.singleLineHeight : GetListHeight(property, drawFoldout) - EditorGUIUtility.singleLineHeight
            };
            bool isDragSingleObject = rectDandD.Contains(e.mousePosition) && itemCount == 1;
            bool isDragMultiObject = rectDandD.Contains(e.mousePosition) && itemCount > 1;

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
                        var exists = new List<Object>();
                        for(int i = 0; i < property.arraySize; i++)
                        {
                            var obj = property.GetArrayElementAtIndex(i);
                            if(!string.IsNullOrEmpty(childName)) obj = obj.FPR(childName);
                            if(obj.objectReferenceValue) exists.Add(obj.objectReferenceValue);
                        }

                        var objectsToAdd = items.Where(o => o && !exists.Contains(o)).ToArray();
                        foreach(var o in objectsToAdd)
                        {
                            property.InsertArrayElementAtIndex(property.arraySize);
                            var current = property.GetArrayElementAtIndex(property.arraySize - 1);
                            if(initializeFunction != null) initializeFunction.Invoke(current);
                            if(!string.IsNullOrEmpty(childName)) current.FPR(childName).objectReferenceValue = o;
                            else current.objectReferenceValue = o;
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

        internal static void DrawDropLect(Rect position)
        {
            dropStyle.fontSize = (int)Mathf.Min(24, position.height);
            EditorGUI.LabelField(position, Localization.G("inspector.dragAndDrop"), dropStyle);
        }

        internal static GUIContent[] CreateContents(string[] labels)
        {
            return labels.Select(l => new GUIContent(l)).ToArray();
        }

        internal static void SetColors(this GUIStyle style, Color color)
        {
            style.active.textColor    = color;
            style.focused.textColor   = color;
            style.hover.textColor     = color;
            style.normal.textColor    = color;
            style.onActive.textColor  = color;
            style.onFocused.textColor = color;
            style.onHover.textColor   = color;
            style.onNormal.textColor  = color;
        }

        internal static void SetColors(this GUIStyle style, Color[] colors)
        {
            style.active.textColor    = colors[0];
            style.focused.textColor   = colors[1];
            style.hover.textColor     = colors[2];
            style.normal.textColor    = colors[3];
            style.onActive.textColor  = colors[4];
            style.onFocused.textColor = colors[5];
            style.onHover.textColor   = colors[6];
            style.onNormal.textColor  = colors[7];
        }

        internal static Color[] GetColors(this GUIStyle style)
        {
            return new[]{
                style.active.textColor   ,
                style.focused.textColor  ,
                style.hover.textColor    ,
                style.normal.textColor   ,
                style.onActive.textColor ,
                style.onFocused.textColor,
                style.onHover.textColor  ,
                style.onNormal.textColor ,
            };
        }

        internal static void SetBackground(this GUIStyle style, Color color)
        {
            var tex = new Texture2D(1,1);
            tex.SetPixel(0,0,color);
            style.SetBackground(tex);
        }

        private static void SetBackground(this GUIStyle style, Texture2D tex)
        {
            style.active.background    = tex;
            style.focused.background   = tex;
            style.hover.background     = tex;
            style.normal.background    = tex;
            style.onActive.background  = tex;
            style.onFocused.background = tex;
            style.onHover.background   = tex;
            style.onNormal.background  = tex;
        }
    }
}
