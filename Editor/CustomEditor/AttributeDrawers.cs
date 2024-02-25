using jp.lilxyzw.avatarmodifier.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
{
    [CustomPropertyDrawer(typeof(MenuFolderOverrideAttribute))]
    internal class MenuFolderOverrideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string parentName;
            var gameObject = (property.serializedObject.targetObject as Component).gameObject;
            if(property.objectReferenceValue)
            {
                parentName = property.objectReferenceValue.name;
            }
            else
            {
                var parent = gameObject.GetComponentInParentInAvatar<MenuFolder>();
                if(parent) parentName = parent.gameObject.name;
                else parentName = "(Root)";
            }
            EditorGUI.LabelField(position.SingleLine(), Localization.G("inspector.parentFolder"), new GUIContent(parentName));
            position.Indent();
            EditorGUI.PropertyField(position.NewLine(), property, Localization.G(property));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GUIHelper.propertyHeight * 2 + GUIHelper.GetSpaceHeight(2);
        }
    }

    [CustomPropertyDrawer(typeof(OneLineVectorAttribute))]
    internal class OneLineVectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            bool wideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            var vec = EditorGUI.Vector4Field(position, Localization.G(property), property.vector4Value);
            EditorGUIUtility.wideMode = wideMode;
            if(EditorGUI.EndChangeCheck())
            {
                property.vector4Value = vec;
            }
        }
    }

    [CustomPropertyDrawer(typeof(NoLabelAttribute))]
    internal class NoLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(MenuNameAttribute))]
    internal class MenuNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIHelper.TextField(position, Localization.G(property), property, property.serializedObject.targetObject.name);
        }
    }

    [CustomPropertyDrawer(typeof(CostumeNameAttribute))]
    internal class CostumeNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(string.IsNullOrEmpty(property.stringValue))
            {
                var copy = property.Copy();
                copy.Next(false);
                copy.Next(false);
                var togglers = copy.FPR("objects");
                string name = null;
                for(int i = 0; i < togglers.arraySize; i++)
                {
                    var obj = togglers.GetArrayElementAtIndex(i).FPR("obj").objectReferenceValue;
                    if(!obj) continue;
                    name = obj.name;
                    break;
                }
                GUIHelper.TextField(position, Localization.G(property), property, name);
            }
            else
            {
                GUIHelper.AutoField(position, property);
            }
        }
    }
}
