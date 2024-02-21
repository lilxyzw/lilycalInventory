using jp.lilxyzw.materialmodifier.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.materialmodifier
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
}
