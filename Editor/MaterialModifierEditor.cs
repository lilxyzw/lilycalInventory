using jp.lilxyzw.materialmodifier.runtime;
using UnityEditor;

namespace jp.lilxyzw.materialmodifier
{
    [CustomEditor(typeof(MaterialModifier))]
    internal class MaterialModifierEditor : Editor
    {
        SerializedProperty ignoreMaterials;
        SerializedProperty referenceMaterial;
        SerializedProperty properties;
        void OnEnable()
        {
            ignoreMaterials = serializedObject.FindProperty("ignoreMaterials");
            referenceMaterial = serializedObject.FindProperty("referenceMaterial");
            properties = serializedObject.FindProperty("properties");
        }
        public override void OnInspectorGUI()
        {
            Localization.SelectLanguageGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(referenceMaterial, Localization.G("inspector.referencematerial"));
            EditorGUILayout.PropertyField(ignoreMaterials, Localization.G("inspector.ignorematerials"));
            EditorGUILayout.PropertyField(properties, Localization.G("inspector.properties"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
