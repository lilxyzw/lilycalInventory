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
            OnDisableInternal();
        }

        internal static void OnDisableInternal()
        {
            GUIHelper.ResetList();
            PreviewHelper.instance.StopPreview();
            if(PreviewHelper.doPreview == 1) PreviewHelper.doPreview = 0;
            ParameterViewer.Reset();
            menuChildren.Clear();
        }

        public override void OnInspectorGUI()
        {
            VersionChecker.DrawGUI();
            Localization.SelectLanguageGUI();

            // AutoDresser用の警告
            // オンになっているオブジェクトが1つだけでない場合に初期衣装を決定できないためエラーを表示
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

            // コンポーネントがオフになっている場合にビルド時に無視される旨の警告を表示
            if(targets.All(t => !((AvatarTagComponent)t).enabled)) EditorGUILayout.HelpBox(Localization.S("inspector.componentDisabled"), MessageType.Info);

            if(target is MenuBaseComponent comp)
            {
                // ExpressionParameters
                ParameterViewer.Draw(comp);

                // プレビューの設定用GUI
                if(targets.Length == 1 && PreviewHelper.instance.ChechTargetHasPreview(target))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    PreviewHelper.instance.TogglePreview(target);
                    PreviewHelper.instance.DrawIndex(target);
                    EditorGUILayout.EndVertical();
                }
            }

            // ----------------------------------------------------------------
            // ここからプロパティの描画
            var hasProperty = false;

            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // Skip m_Script

            // メニュー系コンポーネントである場合はアイコン等を整理して表示
            if(target is MenuBaseComponent)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(Localization.G("inspector.menuSettings"), EditorStyles.boldLabel);
                DrawMenuBaseParameters(target, serializedObject, iterator);
                hasProperty = true;
            }

            // 残りのプロパティを表示
            while(iterator.NextVisible(false))
            {
                if(iterator.name == "costumes" || iterator.name == "frames") GUIHelper.AutoField(iterator, false);
                else GUIHelper.AutoField(iterator);
                hasProperty = true;
            }

            // 変更を適用、変更点がある場合はプレビューを停止して更新
            if(serializedObject.ApplyModifiedProperties()) PreviewHelper.instance.StopPreview();

            // 設定が存在しない場合はその旨を表示
            if(!hasProperty) EditorGUILayout.HelpBox(Localization.S("inspector.noProperty"), MessageType.Info);

            // ----------------------------------------------------------------
            // フォルダの中身を表示
            if(targets.Length == 1 && target is MenuFolder folder)
            {
                if(menuChildren.Count == 0)
                {
                    var root = folder.gameObject.GetAvatarRoot();
                    if(root)
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
                }
                if(menuChildren.ContainsKey(folder) && menuChildren[folder].Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(Localization.G("inspector.folderContents"), EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawChildren(folder);
                    EditorGUILayout.EndVertical();
                }
            }

            // ----------------------------------------------------------------
            // フォルダの中身を表示
            if(target is Prop && GUILayout.Button(Localization.S("inspector.convertToItemToggler")))
                targets.Select(t => t as Prop).Where(t => t).ToArray().PropToToggler(true);

            // ----------------------------------------------------------------
            // 全てを確認した後にプレビューを実行
            if(targets.Length == 1) PreviewHelper.instance.StartPreview(target);
        }

        // メニューの子を再帰的に表示
        private static void DrawChildren(MenuFolder root, MenuFolder current = null)
        {
            EditorGUILayout.BeginVertical();
            var folder = current == null ? root : current;
            var components = menuChildren[folder];
            foreach(var c in components)
            {
                if(c.GetMenuParent() != folder) continue;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                ParamsPerChildren(c);
                if(c == root)
                {
                    EditorGUILayout.HelpBox(Localization.S("inspector.folderContentsCircularReference"), MessageType.Error);
                }
                else if(c is MenuFolder f)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(6, false);
                    DrawChildren(root, f);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        // メニューの子オブジェクトとそのプロパティを表示
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

        // メニューのパラメーターを整理して表示
        private static GUIStyle styleIcon => m_StyleIcon != null ? m_StyleIcon : m_StyleIcon = new GUIStyle(EditorStyles.objectFieldThumb){alignment = TextAnchor.MiddleCenter};
        private static GUIStyle m_StyleIcon;
        private static void DrawMenuBaseParameters(Object obj, SerializedObject so, SerializedProperty iterator)
        {
            EditorGUILayout.BeginHorizontal();
                // アイコンは左に大きく正方形で表示
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

                // MAで制御される場合はグレーアウト
                if(isOverridedByMA) EditorGUI.BeginDisabledGroup(true);
                //parentOverride
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
                // MAで制御される場合はグレーアウト

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
