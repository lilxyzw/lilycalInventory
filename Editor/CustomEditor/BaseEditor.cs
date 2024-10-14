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
        private static string ndmfVersion = "";
        private static string NdmfVersion => string.IsNullOrEmpty(ndmfVersion) ? ndmfVersion = AsmdefReader.Asmdef_LI.versionDefines.FirstOrDefault(v => v.define == "LIL_NDMF").expression : ndmfVersion;
        private static string maVersion = "";
        private static string MAVersion => string.IsNullOrEmpty(maVersion) ? maVersion = AsmdefReader.Asmdef_LI.versionDefines.FirstOrDefault(v => v.define == "LIL_MODULAR_AVATAR").expression : maVersion;
        private static readonly Dictionary<MenuFolder, List<MenuBaseComponent>> menuChildren = new();
        void OnDisable()
        {
            OnDisableInternal();
        }

        internal static void OnDisableInternal()
        {
            PreviewHelper.instance.StopPreview();
            if(PreviewHelper.doPreview == 1) PreviewHelper.doPreview = 0;
            ParameterViewer.Reset();
            AvatarScanner.Reset();
            menuChildren.Clear();
        }

        public override void OnInspectorGUI()
        {
            VersionChecker.DrawGUI();
            Localization.SelectLanguageGUI();

            #if !LIL_MODULAR_AVATAR && LIL_MODULAR_AVATAR_ANY
            EditorGUILayout.HelpBox(string.Format(Localization.S("inspector.maTooOld"), MAVersion), MessageType.Error);
            #endif
            #if !LIL_NDMF && LIL_NDMF_ANY
            EditorGUILayout.HelpBox(string.Format(Localization.S("inspector.ndmfTooOld"), NdmfVersion), MessageType.Error);
            #endif

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
                        using var so = new SerializedObject(d.gameObject);
                        using var sp = so.FindProperty("m_IsActive");
                        var a = PreviewHelper.GetFromContainer(d.gameObject, sp.propertyPath);
                        if(a is bool activeSelf && activeSelf) activeCount++;
                        if(a == null && d.gameObject.activeSelf) activeCount++;
                    }

                    if(activeCount == 0) EditorGUILayout.HelpBox(Localization.S("dialog.error.allObjectOff"), MessageType.Error);
                    if(activeCount > 1) EditorGUILayout.HelpBox(Localization.S("dialog.error.defaultDuplication"), MessageType.Error);
                }
            }

            // コンポーネントがオフになっている場合にビルド時に無視される旨の警告を表示
            if(targets.All(t => !((AvatarTagComponent)t).enabled)) EditorGUILayout.HelpBox(Localization.S("inspector.componentDisabled"), MessageType.Warning);

            // 他メニューが同じオブジェクトについている場合警告を表示
            if(target is Prop || target is AutoDresserSettings)
            {
                MenuBaseComponent dis = (target as Component).gameObject.GetComponent<MenuBaseDisallowMultipleComponent>();
                if(!dis && target is Prop) dis = (target as Component).gameObject.GetComponent<AutoDresserSettings>();
                if(!dis && target is AutoDresserSettings) dis = (target as Component).gameObject.GetComponent<Prop>();
                if(dis) EditorGUILayout.HelpBox(string.Format(Localization.S("inspector.componentDuplicate"), target.GetType().Name, dis.GetType().Name), MessageType.Error);
            }

            if(target is MenuBaseComponent comp)
            {
                // コンポーネントの親フォルダがオフの場合にビルド時に無視される旨の警告を表示
                var unenabled = ObjHelper.UnenabledParent(comp);
                if(unenabled)
                {
                    EditorGUILayout.HelpBox(Localization.S("inspector.parentDisabled"), MessageType.Warning);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(unenabled, typeof(Object), true);
                    EditorGUI.EndDisabledGroup();
                }

                // ExpressionParameters
                ParameterViewer.Draw(comp);
                AvatarScanner.Update(comp);
                AvatarScanner.Draw(targets);

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
            using var iterator = serializedObject.GetIterator();
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
            // Prop => ItemToggler 変換ボタン
            if(target is Prop && GUILayout.Button(Localization.S("inspector.convertToItemToggler")))
                foreach(var prop in targets.Select(t => t as Prop).Where(t => t).ToArray())
                    Processor.PropToToggler(new[]{prop}, prop.gameObject.GetAvatarRoot().GetComponentsInChildren<Preset>(true));

            // ----------------------------------------------------------------
            // フォルダ生成ボタン
            if((target is AutoDresser || target is Prop) && !(target as MenuBaseComponent).parentOverride && GUILayout.Button(Localization.S("inspector.generateMenuFolder")))
                foreach(var c in targets.Select(t => t as MenuBaseComponent).Where(t => t && !t.parentOverride && (t is AutoDresser || t is Prop)).ToArray())
                {
                    Undo.RecordObject(c, "Generate Folder");
                    c.parentOverride = Undo.AddComponent<MenuFolder>(c.gameObject);
                }

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

            using var so = new SerializedObject(obj);
            so.UpdateIfRequiredOrScript();

            using var iterator = so.GetIterator();
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
                #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
                bool isOverridedByMA = so.GetObjectInProperty("parentOverrideMA");
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
                #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
                GUIHelper.AutoField(iterator);
                #endif
                EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ModularAvatarHelper.Inspector(obj, iterator);
        }

        // オブジェクトの参照がアバター外のオブジェクトになっている場合、アバター内に修正
        [InitializeOnLoadMethod] private static void Initialize() => ObjectChangeEvents.changesPublished += (ref ObjectChangeEventStream stream) =>
        {
            var components = new HashSet<AvatarTagComponent>();
            for(int i = 0; i < stream.length; i++)
            {
                switch(stream.GetEventType(i))
                {
                    case ObjectChangeKind.ChangeGameObjectParent:
                    {
                        stream.GetChangeGameObjectParentEvent(i, out var data);
                        if(EditorUtility.InstanceIDToObject(data.instanceId) is GameObject go) components.UnionWith(go.GetComponentsInChildren<AvatarTagComponent>(true));
                        break;
                    }
                    case ObjectChangeKind.ChangeGameObjectStructure:
                    {
                        stream.GetChangeGameObjectStructureEvent(i, out var data);
                        if(EditorUtility.InstanceIDToObject(data.instanceId) is GameObject go) components.UnionWith(go.GetComponents<AvatarTagComponent>());
                        break;
                    }
                    default: continue;
                }
            }

            if(components.Count == 0) return;
            foreach(var component in components) FixObjectReferences(component);
        };

        private static void FixObjectReferences(AvatarTagComponent component)
        {
            if(!component || !component.gameObject ||
                component is Comment ||
                component is MaterialModifier ||
                component is MaterialOptimizer
            ) return;
            var root = component.gameObject.GetAvatarRoot();
            if(!root) return;
            using var so = new SerializedObject(component);
            using var iter = so.GetIterator();
            var enterChildren = true;
            while(iter.Next(enterChildren))
            {
                enterChildren = iter.propertyType != SerializedPropertyType.String;
                if(iter.propertyType != SerializedPropertyType.ObjectReference) continue;
                if(iter.objectReferenceValue is GameObject gameObject && gameObject.GetAvatarRoot() != root)
                {
                    var lastPath = gameObject.GetPathInAvatar();
                    if(string.IsNullOrEmpty(lastPath)) continue;
                    iter.objectReferenceValue = root.transform.Find(lastPath).gameObject;
                }
                else if(iter.objectReferenceValue is Component c && c.gameObject.GetAvatarRoot() != root)
                {
                    var lastPath = c.GetPathInAvatar();
                    if(string.IsNullOrEmpty(lastPath)) continue;
                    var t = root.transform.Find(lastPath);
                    if(t) iter.objectReferenceValue = t.GetComponent(c.GetType());
                    else iter.objectReferenceValue = null;
                }
            }
            so.ApplyModifiedProperties();
        }
    }
}
