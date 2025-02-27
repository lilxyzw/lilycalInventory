using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class GUIHelper
    {
        // リストのかわりにプロパティを並べて表示するだけのシンプルなもの
        internal static Rect SimpleList(SerializedProperty prop, Rect position, Material[] originalMaterials)
        {
            using var endProperty = prop.GetEndProperty();
            prop.NextVisible(true);
            position.Back();
            int i = 0;
            while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProperty))
            {
                var rect = position.NewLine();

                var materialRect = rect;
                materialRect.width = rect.width * 0.5f - 2;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ObjectField(materialRect, originalMaterials[i], typeof(Material), false);
                EditorGUI.EndDisabledGroup();

                var replaceRect = rect;
                replaceRect.x += materialRect.width + 4;
                replaceRect.width = rect.width * 0.5f - 2;
                EditorGUI.PropertyField(replaceRect, prop, GUIContent.none);

                i++;
            }
            return position;
        }
    }
}
