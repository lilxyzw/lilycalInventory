using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class GUIHelper
    {
        // リストのかわりにプロパティを並べて表示するだけのシンプルなもの
        internal static Rect SimpleList(SerializedProperty prop, Rect position, string[] labels)
        {
            using var endProperty = prop.GetEndProperty();
            prop.NextVisible(true);
            position.Back();
            int i = 0;
            while(prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProperty))
            {
                EditorGUI.PropertyField(position.NewLine(), prop, new GUIContent(labels[i++]));
            }
            return position;
        }
    }
}
