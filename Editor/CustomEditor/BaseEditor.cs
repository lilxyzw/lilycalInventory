using jp.lilxyzw.avatarmodifier.runtime;
using UnityEditor;

namespace jp.lilxyzw.avatarmodifier
{
    [CustomEditor(typeof(AvatarTagComponent), true)] [CanEditMultipleObjects]
    internal class BaseEditor : Editor
    {
        void OnDisable() => GUIHelper.ResetList();

        public override void OnInspectorGUI()
        {
            Localization.SelectLanguageGUI();
            var hasProperty = false;

            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // Skip m_Script
            while(iterator.NextVisible(false))
            {
                GUIHelper.AutoField(iterator);
                hasProperty = true;
            }
            serializedObject.ApplyModifiedProperties();

            if(!hasProperty) EditorGUILayout.HelpBox(Localization.S("inspector.noProperty"), MessageType.Info);
        }
    }
}
