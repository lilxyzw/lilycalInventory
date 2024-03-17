using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class GUIHelper
    {
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
        internal static GUIStyle boldRedStyle
        {
            get
            {
                if(m_BoldRedStyle == null) InitializeGUI();
                return m_BoldRedStyle;
            }
        }
        private static GUIStyle m_PlaceholderStyle;
        private static GUIStyle m_PlaceholderObjectStyle;
        private static GUIStyle m_DropStyle;
        private static GUIStyle m_BoldRedStyle;

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
            m_BoldRedStyle = new GUIStyle(EditorStyles.boldLabel);
            var col = EditorStyles.objectField.normal.textColor;
            SetColors(m_PlaceholderObjectStyle, new Color(col.r, col.g, col.b, 0.5f));
            SetColors(m_BoldRedStyle, Color.red);
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
