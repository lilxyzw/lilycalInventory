using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    [CustomEditor(typeof(AvatarTagComponent), true)] [CanEditMultipleObjects]
    internal class BaseEditor : Editor
    {
        private static Dictionary<MenuFolder, List<MenuBaseComponent>> menuChildren = new Dictionary<MenuFolder, List<MenuBaseComponent>>();
        void OnDisable()
        {
            GUIHelper.ResetList();
            PreviewHelper.instance.StopPreview();
            ParameterViewer.Reset();
            menuChildren.Clear();
        }

        public override void OnInspectorGUI()
        {
            Localization.SelectLanguageGUI();

            // Warn for AutoDresser
            if(target is AutoDresser dresser)
            {
                var root = dresser.gameObject.GetAvatarRoot();
                if(root)
                {
                    int activeCount = 0;

                    foreach(var d in root.GetComponentsInChildren<AutoDresser>(true))
                    {
                        if(!d.enabled || d.IsEditorOnly()) continue;
                        var a = PreviewHelper.GetFromContainer(d.gameObject, new SerializedObject(d.gameObject).FindProperty("m_IsActive").propertyPath);
                        if(a is bool activeSelf && activeSelf) activeCount++;
                        if(a == null && d.gameObject.activeSelf) activeCount++;
                    }

                    if(activeCount == 0) EditorGUILayout.HelpBox(Localization.S("dialog.error.allObjectOff"), MessageType.Error);
                    if(activeCount > 1) EditorGUILayout.HelpBox(Localization.S("dialog.error.defaultDuplication"), MessageType.Error);
                }
            }

            // Warn component disabled
            if(targets.All(t => !((AvatarTagComponent)t).enabled)) EditorGUILayout.HelpBox(Localization.S("inspector.componentDisabled"), MessageType.Info);

            // ExpressionParameters
            if(target is MenuBaseComponent comp) ParameterViewer.Draw(comp);

            // Preview Helper
            if(targets.Length == 1 && PreviewHelper.instance.ChechTargetHasPreview(target))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(Localization.G("inspector.previewAnimation"));
                PreviewHelper.instance.DrawIndex(target);
                PreviewHelper.instance.TogglePreview(target);
                EditorGUILayout.EndVertical();
            }

            // Draw Properties
            var hasProperty = false;

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

            // Folder Viewer
            if(targets.Length == 1 && target is MenuFolder folder)
            {
                if(menuChildren.Count == 0)
                {
                    var components = folder.gameObject.GetAvatarRoot().GetComponentsInChildren<MenuBaseComponent>(true).Where(c => c.enabled);
                    foreach(var c in components)
                    {
                        if(c is MenuFolder f && !menuChildren.ContainsKey(f)) menuChildren[f] = new List<MenuBaseComponent>();
                        var parent = c.GetMenuParent();
                        if(!parent) continue;
                        if(!menuChildren.ContainsKey(parent)) menuChildren[parent] = new List<MenuBaseComponent>();
                        menuChildren[parent].Add(c);
                    }
                }
                if(menuChildren[folder].Count > 0)
                {
                    EditorGUILayout.LabelField(Localization.G("inspector.folderContents"));
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawChildren(folder);
                    EditorGUILayout.EndVertical();
                }
            }

            if(targets.Length == 1) PreviewHelper.instance.StartPreview(target);
        }

        internal static void DrawChildren(MenuFolder folder)
        {
            EditorGUILayout.BeginVertical();
            var components = menuChildren[folder];
            foreach(var c in components)
            {
                if(c.GetMenuParent() != folder) continue;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                ParamsPerChildren(c);
                if(c is MenuFolder f)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(6, false);
                    DrawChildren(f);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        internal static void ParamsPerChildren(MenuBaseComponent obj)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(obj, obj.GetType(), true);
            EditorGUI.EndDisabledGroup();

            var so = new SerializedObject(obj);
            so.UpdateIfRequiredOrScript();

            EditorGUILayout.BeginHorizontal();
                var iconSize = EditorGUIUtility.singleLineHeight * 3 + GUIHelper.GetSpaceHeight(3);
                var icon = so.FindProperty("icon");
                EditorGUI.BeginChangeCheck();
                var tex = EditorGUILayout.ObjectField(icon.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(iconSize), GUILayout.Height(iconSize));
                if(EditorGUI.EndChangeCheck()) icon.objectReferenceValue = tex;

                EditorGUILayout.BeginVertical();
                var iterator = so.GetIterator();
                iterator.NextVisible(true);
                if(!ModularAvatarHelper.Inspector(obj, so, iterator, true))
                {
                    EditorGUILayout.PropertyField(so.FindProperty("menuName"));
                    EditorGUILayout.PropertyField(so.FindProperty("parentOverride"));
                }
                EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            so.ApplyModifiedProperties();
        }
    }
}
