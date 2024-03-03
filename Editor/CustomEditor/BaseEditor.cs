using System.Linq;
using UnityEditor;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

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

            if(targets.All(t => !((AvatarTagComponent)t).enabled)) EditorGUILayout.HelpBox(Localization.S("inspector.componentDisabled"), MessageType.Info);

            if(targets.Length == 1 && PreviewHelper.instance.ChechTargetHasPreview(target))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(Localization.G("inspector.previewAnimation"));
                PreviewHelper.instance.DrawIndex(target);
                PreviewHelper.instance.TogglePreview(target);
                EditorGUILayout.EndVertical();
            }

            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // Skip m_Script
            hasProperty = ModularAvatarHelper.Inspector(target, serializedObject, iterator) || hasProperty;
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
