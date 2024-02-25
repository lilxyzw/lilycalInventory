using jp.lilxyzw.avatarmodifier.runtime;
using UnityEditor;

namespace jp.lilxyzw.avatarmodifier
{
    [CustomEditor(typeof(AvatarTagComponent), true)] [CanEditMultipleObjects]
    internal class BaseEditor : Editor
    {
        void OnDisable()
        {
            GUIHelper.ResetList();
            PreviewHelper.instance.StopPreview();
        }

        public override void OnInspectorGUI()
        {
            Localization.SelectLanguageGUI();
            var hasProperty = false;

            if(targets.Length == 1)
            {
                PreviewHelper.instance.TogglePreview(target);
                PreviewHelper.instance.DrawIndex(target);
            }

            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // Skip m_Script
            while(iterator.NextVisible(false))
            {
                GUIHelper.AutoField(iterator);
                hasProperty = true;
            }
            if(serializedObject.ApplyModifiedProperties()) PreviewHelper.instance.StopPreview();

            if(!hasProperty) EditorGUILayout.HelpBox(Localization.S("inspector.noProperty"), MessageType.Info);

            if(targets.Length == 1) PreviewHelper.instance.StartPreview(target);
        }
    }
}
