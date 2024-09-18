using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;
    using UnityEditor;
    using UnityEditor.Animations;

    internal static class Processor
    {
        // lilycalInventoryの処理を行うか
        private static bool shouldModify;

        // Material系コンポーネント
        private static MaterialModifier[] modifiers;
        private static MaterialOptimizer[] optimizers;

        // Renderer系コンポーネント
        private static AutoFixMeshSettings[] meshSettings;

        // メニュー系コンポーネント
        private static AvatarTagComponent[] components;
        private static AutoDresserSettings[] dresserSettings;
        private static AutoDresser[] dressers;
        private static Prop[] props;
        private static MenuFolder[] folders;
        private static ItemToggler[] togglers;
        private static CostumeChanger[] costumeChangers;
        private static SmoothChanger[] smoothChangers;
        private static Preset[] presets;
        private static MenuBaseComponent[] menuBaseComponents;
        private static Material[] materials;
        private static HashSet<string> parameterNames;

        // 強制アクティブにするオブジェクトとそのオブジェクトの元のアクティブ状態の配列
        private static (GameObject,bool)[] actives;

        internal static void FindComponent(BuildContext ctx)
        {
            Cloner.materialMap = new Dictionary<Material, Material>();

            // Commentコンポーネントを削除
            foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<Comment>(true))
                Object.DestroyImmediate(component);
            
            presets = ctx.AvatarRootObject.GetActiveComponentsInChildren<Preset>(true);

            var deleteDressers = new HashSet<AutoDresser>();
            foreach(var changer in ctx.AvatarRootObject.GetActiveComponentsInChildren<CostumeChanger>(true))
            {
                for(int i = 0; i < changer.costumes.Length; i++)
                {
                    var costume = changer.costumes[i];
                    if(!costume.autoDresser) continue;
                    costume.parametersPerMenu = costume.parametersPerMenu.Merge(costume.autoDresser.parameter);
                    costume.parametersPerMenu.objects = costume.parametersPerMenu.objects.Append(new ObjectToggler{obj = costume.autoDresser.gameObject, value = true}).ToArray();
                    if(string.IsNullOrEmpty(costume.menuName)) costume.menuName = costume.autoDresser.GetMenuName();
                    deleteDressers.Add(costume.autoDresser);
                    presets.ReplaceComponent(costume.autoDresser, changer, i);
                }
            }
            foreach(var d in deleteDressers) Object.DestroyImmediate(d);

            dressers = ctx.AvatarRootObject.GetActiveComponentsInChildren<AutoDresser>(true);
            props = ctx.AvatarRootObject.GetActiveComponentsInChildren<Prop>(true);
            actives = dressers.Select(d => (d.gameObject,d.gameObject.activeSelf)).Union(props.Select(p => (p.gameObject,p.gameObject.activeSelf))).ToArray();

            // AutoDresserをCostumeChangerに変換
            dresserSettings = ctx.AvatarRootObject.GetActiveComponentsInChildren<AutoDresserSettings>(false);
            if(dresserSettings.Length > 1) ErrorHelper.Report("dialog.error.dresserSettingsDuplicate", dresserSettings);
            dressers.ResolveMenuName();
            dressers.DresserToChanger(dresserSettings, presets);

            // PropをItemTogglerに変換
            props.ResolveMenuName();
            props.PropToToggler(presets);

            // メニュー名に応じてフォルダ生成
            foreach(var c in ctx.AvatarRootObject.GetActiveComponentsInChildren<MenuBaseComponent>(true).ToArray())
            {
                var siblingIndex = c.transform.GetSiblingIndex();

                MenuFolder SlashToFolder(string[] names, MenuFolder rootParent)
                {
                    MenuFolder firstFolder = null;
                    MenuFolder lastFolder = null;
                    for(int i = names.Length-2; i >= 0; i--)
                    {
                        var name = names[i];
                        var obj = new GameObject(name);
                        obj.transform.parent = c.transform.parent;
                        obj.transform.SetSiblingIndex(siblingIndex++);
                        var folder = obj.AddComponent<MenuFolder>();
                        if(lastFolder) lastFolder.parentOverride = folder;
                        else firstFolder = folder;

                        if(i == 0) folder.parentOverride = rootParent;
                        lastFolder = folder;
                    }
                    return firstFolder;
                }

                var namesP = c.menuName.Split('/');
                if(namesP.Length > 1)
                {
                    c.parentOverride = SlashToFolder(namesP, c.GetMenuParent());
                    c.menuName = namesP[namesP.Length-1];
                }

                if(c is CostumeChanger cc)
                {
                    foreach(var costume in cc.costumes)
                    {
                        var namesC = costume.menuName.Split('/');
                        if(namesC.Length <= 1) continue;
                        costume.parentOverride = SlashToFolder(namesC, costume.parentOverride);
                        costume.menuName = namesC[namesC.Length-1];
                    }
                }
            }

            // 一時的にアクティブにして子のコンポーネントが探索されるようにする
            foreach(var a in actives) a.Item1.SetActive(true);
            components = ctx.AvatarRootObject.GetActiveComponentsInChildren<AvatarTagComponent>(true).ToArray();
            foreach(var a in actives) a.Item1.SetActive(a.Item2);

            // 各コンポーネントを取得
            modifiers = components.SelectComponents<MaterialModifier>();
            optimizers = components.SelectComponents<MaterialOptimizer>();
            meshSettings = components.SelectComponents<AutoFixMeshSettings>();
            folders = components.SelectComponents<MenuFolder>();
            togglers = components.SelectComponents<ItemToggler>();
            costumeChangers = components.SelectComponents<CostumeChanger>();
            smoothChangers = components.SelectComponents<SmoothChanger>();
            presets = components.SelectComponents<Preset>();
            menuBaseComponents = components.SelectComponents<MenuBaseComponent>();
            shouldModify = components.Length != 0;
            if(!shouldModify) return;

            // メニュー名を解決
            menuBaseComponents.ResolveMenuName();
            costumeChangers.ResolveMenuName();

            // 全メッシュに処理を適用するかを確認
            ObjHelper.CheckApplyToAll(togglers, costumeChangers, smoothChangers);

            // 再帰的にMenuGroup配下に
            ModularAvatarHelper.ResolveMenu(folders, togglers, costumeChangers, smoothChangers, presets);
        }

        // アニメーション系コンポーネントの処理
        internal static void ModifyPreProcess(BuildContext ctx)
        {
            if(!shouldModify) return;
            ObjHelper.pathInAvatars.Clear();
            #if LIL_VRCSDK3A
            if(togglers.Length + costumeChangers.Length + smoothChangers.Length + presets.Length > 0)
            {
                var controller = ctx.AvatarDescriptor.TryGetFXAnimatorController(ctx);
                var hasWriteDefaultsState = controller.HasWriteDefaultsState();
                var menu = VRChatHelper.CreateMenu(ctx);
                var parameters = VRChatHelper.CreateParameters(ctx);

                // パラメーター名の重複チェック
                parameterNames = new HashSet<string>();
                if(controller.parameters != null) parameterNames.UnionWith(controller.parameters.Select(p => p.name));
                if(ctx.AvatarDescriptor.expressionParameters) ctx.AvatarDescriptor.expressionParameters.parameters.Select(p => p.name);
                var parameterDuplicates = menuBaseComponents.Where(c => c is IGenerateParameter && parameterNames.Contains(c.menuName)).Select(c => c.gameObject).ToArray();
                if(parameterDuplicates.Length > 0) ErrorHelper.Report("dialog.error.parameterDuplication", parameterDuplicates);

                // DirectBlendTreeのルートを作成
                BlendTree tree = null;
                if(ToolSettings.instance.useDirectBlendTree)
                {
                    AnimationHelper.CreateLayer(controller, out tree);
                    AssetDatabase.AddObjectToAsset(tree, ctx.AssetContainer);
                }

                // 各コンポーネントの処理
                Modifier.ResolveMultiConditions(ctx, controller, hasWriteDefaultsState, togglers, costumeChangers, tree);
                Modifier.ApplyItemToggler(ctx, controller, hasWriteDefaultsState, togglers, tree, parameters);
                Modifier.ApplyCostumeChanger(ctx, controller, hasWriteDefaultsState, costumeChangers, tree, parameters);
                Modifier.ApplySmoothChanger(ctx, controller, hasWriteDefaultsState, smoothChangers, tree, parameters);
                Modifier.ApplyPreset(ctx, controller, hasWriteDefaultsState, presets, parameters);

                // DirectBlendTreeの子のパラメーターを設定
                if(ToolSettings.instance.useDirectBlendTree) AnimationHelper.SetParameter(tree);

                // メニューを生成
                MenuGenerator.Generate(ctx, folders, togglers, smoothChangers, costumeChangers, presets, menuBaseComponents, menu);

                // 生成したメニュー・パラメーターをマージ
                ctx.AvatarDescriptor.MergeParameters(menu, parameters, ctx);
            }
            #else
            // 各SNSごとに処理を追加していく必要あり
            #endif
        }

        // マテリアルの編集とメッシュ設定の統一
        internal static void ModifyPostProcess(BuildContext ctx)
        {
            if(!shouldModify) return;
            materials = Cloner.CloneAllMaterials(ctx);
            Modifier.ApplyMaterialModifier(materials, modifiers);
            Modifier.ApplyMeshSettingsModifier(ctx.AvatarRootObject, meshSettings);
        }

        // 後の最適化ツールが正しく動作するようにコンポーネントをここで除去
        internal static void RemoveComponent(BuildContext ctx)
        {
            foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true))
                Object.DestroyImmediate(component);
        }

        // マテリアルから不要なプロパティを除去
        internal static void Optimize(BuildContext ctx)
        {
            if(shouldModify && optimizers.Length != 0) Optimizer.OptimizeMaterials(materials, ctx);
        }
    }
}
