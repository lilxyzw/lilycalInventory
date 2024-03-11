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

            if(target is MenuBaseComponent comp)
            {
                // ExpressionParameters
                ParameterViewer.Draw(comp);

                // Preview Helper
                if(targets.Length == 1 && PreviewHelper.instance.ChechTargetHasPreview(target))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    PreviewHelper.instance.TogglePreview(target);
                    PreviewHelper.instance.DrawIndex(target);
                    EditorGUILayout.EndVertical();
                }
            }

            // Draw Properties
            var hasProperty = false;

            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // Skip m_Script
            if(target is MenuBaseComponent)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Localization.G("inspector.menuSettings"), EditorStyles.boldLabel);
                DrawMenuBaseParameters(target, serializedObject, iterator);
                hasProperty = true;
            }
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
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(Localization.G("inspector.folderContents"), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawChildren(folder);
                    EditorGUILayout.EndVertical();
                }
            }

            if(targets.Length == 1) PreviewHelper.instance.StartPreview(target);
        }

        private static void DrawChildren(MenuFolder folder)
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

        private static void ParamsPerChildren(MenuBaseComponent obj)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(obj, obj.GetType(), true);
            EditorGUI.EndDisabledGroup();

            var so = new SerializedObject(obj);
            so.UpdateIfRequiredOrScript();

            var iterator = so.GetIterator();
            iterator.NextVisible(true);
            DrawMenuBaseParameters(obj, so, iterator);

            so.ApplyModifiedProperties();
        }

        private static GUIStyle styleIcon => m_StyleIcon != null ? m_StyleIcon : m_StyleIcon = new GUIStyle(EditorStyles.objectFieldThumb){alignment = TextAnchor.MiddleCenter};
        private static GUIStyle m_StyleIcon;
        private static void DrawMenuBaseParameters(Object obj, SerializedObject so, SerializedProperty iterator)
        {
            EditorGUILayout.BeginHorizontal();
                var iconSize = EditorGUIUtility.singleLineHeight * 3 + GUIHelper.GetSpaceHeight(3);
                var rectIcon = EditorGUILayout.GetControlRect(GUILayout.Width(iconSize), GUILayout.Height(iconSize));

                EditorGUILayout.BeginVertical();
                #if LIL_MODULAR_AVATAR
                bool isOverridedByMA = so.FindProperty("parentOverrideMA").objectReferenceValue;
                #else
                bool isOverridedByMA = false;
                #endif
                //menuName
                iterator.NextVisible(false);
                GUIHelper.AutoField(iterator);
                //parentOverride
                if(isOverridedByMA) EditorGUI.BeginDisabledGroup(true);
                iterator.NextVisible(false);
                GUIHelper.AutoField(iterator);
                //icon
                iterator.NextVisible(false);
                EditorGUI.BeginChangeCheck();
                var tex = EditorGUI.ObjectField(rectIcon, iterator.objectReferenceValue, typeof(Texture2D), false);
                if(EditorGUI.EndChangeCheck()) iterator.objectReferenceValue = tex;
                if(!isOverridedByMA && !iterator.objectReferenceValue)
                {
                    EditorGUI.LabelField(rectIcon, Localization.G("inspector.icon"), styleIcon);
                    GUIStyle styleOverlay = EditorStyles.objectFieldThumb.name + "Overlay2";
                    EditorGUI.LabelField(rectIcon, "Select", styleOverlay);
                }
                if(isOverridedByMA) EditorGUI.EndDisabledGroup();
                //parentOverrideMA
                iterator.NextVisible(false);
                #if LIL_MODULAR_AVATAR
                GUIHelper.AutoField(iterator);
                #endif
                EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ModularAvatarHelper.Inspector(obj, iterator);
        }
    }
}
