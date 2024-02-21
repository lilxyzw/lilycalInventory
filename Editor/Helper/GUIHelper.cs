using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.materialmodifier
{
    internal static partial class GUIHelper
    {
        private const float INDENT_WIDTH = 15f;
        private static readonly float GUI_SPACE = EditorGUIUtility.standardVerticalSpacing;
        private static readonly GUIContent hideContent = new GUIContent(" ");
        private static readonly GUIStyle placeholderStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Italic,
            padding = EditorStyles.textField.padding
        };
        internal static readonly float propertyHeight = EditorGUIUtility.singleLineHeight;

        private static bool Foldout(Rect position, SerializedProperty prop, GUIContent content = null)
        {
            var label = EditorGUI.BeginProperty(position, content ?? Localization.G(prop), prop);
            prop.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, prop.isExpanded, label);
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();
            return prop.isExpanded;
        }

        private static bool Foldout(SerializedProperty prop, GUIContent content = null)
        {
            return Foldout(EditorGUILayout.GetControlRect(), prop, content);
        }

        // TODO
        internal static bool FoldoutOnly(Rect position, SerializedProperty prop)
        {
            position.width = 12;
            position.height = propertyHeight;
            position.x -= 12;
            var label = EditorGUI.BeginProperty(position, GUIContent.none, prop);
            position.x += 12;
            prop.isExpanded = EditorGUI.Foldout(position, prop.isExpanded, label);
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();
            return prop.isExpanded;
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

        internal static Rect AutoField(Rect position, SerializedProperty property)
        {
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                return List(position, property.Copy());
            }
            else
            {
                EditorGUI.PropertyField(position.SetHeight(property), property);
                return position.NewLine();
            }
        }

        internal static void AutoField(SerializedProperty property)
        {
            if(property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                List(property.Copy());
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
    }
}
