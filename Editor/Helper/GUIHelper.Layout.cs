using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class GUIHelper
    {
        // 行間
        internal static float GetSpaceHeight(int count = 1)
        {
            return GUI_SPACE * count;
        }

        // 高さを1行ぶんにセット
        internal static Rect SingleLine(this ref Rect position)
        {
            position.height = propertyHeight;
            return position;
        }

        // 高さをプロパティの高さにセット
        internal static Rect SetHeight(this ref Rect position, SerializedProperty property)
        {
            position.height = EditorGUI.GetPropertyHeight(property);
            return position;
        }

        // 高さをリストの高さにセット
        internal static Rect SetHeightList(this ref Rect position, SerializedProperty property)
        {
            position.height = GetListHeight(property) + GetSpaceHeight();
            return position;
        }

        // 改行
        internal static Rect NewLine(this ref Rect position)
        {
            position.y = position.yMax + GUI_SPACE;
            return position;
        }

        internal static Rect NewLine(this ref Rect position, SerializedProperty property)
        {
            position.NewLine();
            position.SetHeight(property);
            return position;
        }

        // 前の行に戻る
        internal static Rect Back(this ref Rect position)
        {
            position.y -= position.height + GUI_SPACE;
            return position;
        }

        // インデントを入れる
        internal static Rect Indent(this ref Rect position, int count = 1)
        {
            position.x += INDENT_WIDTH * count;
            position.width -= INDENT_WIDTH * count;
            return position;
        }
    }
}
