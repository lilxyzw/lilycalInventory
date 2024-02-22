using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
{
    internal static partial class GUIHelper
    {
        internal static Rect SimpleList(SerializedProperty prop, Rect position, string[] labels)
        {
            SerializedProperty endProperty = prop.GetEndProperty();
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
