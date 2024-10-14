using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private static BuildContext ctx;

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
        private static AsMAMergeAnimator asMAMergeAnimator;

        // マテリアルプロパティを1度取得したらその値を保存
        private static readonly Dictionary<(Material material, string name), float> floatValues = new();
        private static readonly Dictionary<(Material material, string name), Vector4> vectorValues = new();

        // 強制アクティブにするオブジェクトとそのオブジェクトの元のアクティブ状態の配列
        private static (GameObject,bool)[] actives;

        internal static void FindComponent(BuildContext context)
        {
            ctx = context;
            Cloner.materialMap.Clear();
            floatValues.Clear();
            vectorValues.Clear();

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
            components = ctx.AvatarRootObject.GetActiveComponentsInChildren<AvatarTagComponent>(true).Where(c => c is not MenuBaseComponent mbc || !mbc.UnenabledParent()).ToArray();
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
            asMAMergeAnimator = components.FirstOrDefault(c => c is AsMAMergeAnimator) as AsMAMergeAnimator;
            shouldModify = components.Length != 0;
            if(!shouldModify) return;

            // メニュー名を解決
            menuBaseComponents.ResolveMenuName();
            costumeChangers.ResolveMenuName();

            // MaterialModifierの値を取得
            Modifier.GetModifierValues(modifiers);

            // 全メッシュに処理を適用するかを確認
            ObjHelper.CheckApplyToAll(togglers, costumeChangers, smoothChangers);

            // 再帰的にMenuGroup配下に
            ModularAvatarHelper.ResolveMenu(folders, togglers, costumeChangers, smoothChangers, presets);
        }

        // アニメーション系コンポーネントの処理
        internal static void ModifyPreProcess(BuildContext context)
        {
            ctx = context;

            if(!shouldModify) return;
            ObjHelper.pathInAvatars.Clear();
            if(togglers.Length + costumeChangers.Length + smoothChangers.Length + presets.Length > 0)
            {
                bool shouldMakeNewController = false;
                #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
                shouldMakeNewController = asMAMergeAnimator;
                #endif

                AnimatorController controller;
                if(shouldMakeNewController)
                {
                    controller = new AnimatorController();
                    AssetDatabase.AddObjectToAsset(controller, ctx.AssetContainer);
                }
                #if LIL_VRCSDK3A
                else controller = VRChatUtils.TryGetFXAnimatorController();
                #else
                // 各SNSごとに処理を追加していく必要あり
                else
                {
                    controller = new AnimatorController();
                    AssetDatabase.AddObjectToAsset(controller, ctx.AssetContainer);
                }
                #endif

                var hasWriteDefaultsState = controller.HasWriteDefaultsState();
                var parameters = new List<InternalParameter>();

                // パラメーター名の重複回避
                Modifier.ResolveParameterNames(controller, togglers, costumeChangers, smoothChangers, presets);

                // DirectBlendTreeのルートを作成
                BlendTree tree = null;
                if(ToolSettings.instance.useDirectBlendTree)
                {
                    AnimationHelper.CreateLayer(controller, out tree);
                    AssetDatabase.AddObjectToAsset(tree, ctx.AssetContainer);
                }

                // 各コンポーネントの処理
                Modifier.ResolveMultiConditions(controller, hasWriteDefaultsState, togglers, costumeChangers, tree);
                Modifier.ApplyItemToggler(controller, hasWriteDefaultsState, togglers, tree, parameters);
                Modifier.ApplyCostumeChanger(controller, hasWriteDefaultsState, costumeChangers, tree, parameters);
                Modifier.ApplySmoothChanger(controller, hasWriteDefaultsState, smoothChangers, tree, parameters);
                Modifier.ApplyPreset(controller, hasWriteDefaultsState, presets, parameters);

                // DirectBlendTreeの子のパラメーターを設定
                if(ToolSettings.instance.useDirectBlendTree) AnimationHelper.SetParameter(tree);

                // メニューを生成
                var menu = MenuGenerator.Generate(folders, togglers, smoothChangers, costumeChangers, presets, menuBaseComponents);

                // 生成したメニュー・パラメーターをマージ
                #if LIL_VRCSDK3A
                VRChatUtils.Apply(menu, parameters);
                #else
                // 各SNSごとに処理を追加していく必要あり
                #endif

                #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
                if(shouldMakeNewController) ModularAvatarHelper.ToMergeAnimator(asMAMergeAnimator, controller);
                #endif
            }
        }

        // マテリアルの編集とメッシュ設定の統一
        internal static void ModifyPostProcess(BuildContext context)
        {
            ctx = context;

            if(!shouldModify) return;
            if(modifiers.Length > 0 || optimizers.Length > 0)
                materials = Cloner.CloneAllMaterials();
            Modifier.ApplyMaterialModifier(materials);
            Modifier.ApplyMeshSettingsModifier(ctx.AvatarRootObject, meshSettings);
        }

        // 後の最適化ツールが正しく動作するようにコンポーネントをここで除去
        internal static void RemoveComponent(BuildContext context)
        {
            ctx = context;

            foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true))
                Object.DestroyImmediate(component);
        }

        // マテリアルから不要なプロパティを除去
        internal static void Optimize(BuildContext context)
        {
            ctx = context;

            if(!shouldModify || optimizers.Length == 0) return;
            #if LIL_NDMF
            // NDMF環境では動作タイミングが分かれているため再取得
            materials = Cloner.CloneAllMaterials();
            #endif
            Optimizer.OptimizeMaterials(materials);
        }

        // 汎用クローン
        private static Object CloneObject(Object obj, BuildContext ctx, Dictionary<Object,Object> map)
        {
            if(!obj || ctx.IsTemporaryAsset(obj)) return obj;
            if(map.ContainsKey(obj)) return map[obj];
            var clone = Object.Instantiate(obj);
            RegisterReplacedObject(obj, clone);
            AssetDatabase.AddObjectToAsset(clone, ctx.AssetContainer);
            return map[obj] = clone;
        }

        private static void RegisterReplacedObject(Object origin, Object clone)
        {
            #if LIL_NDMF
            ObjectRegistry.RegisterReplacedObject(origin, clone);
            #endif
        }

        private static bool OriginEquals(Material origin, Material clone)
        {
            #if LIL_NDMF
            return ObjectRegistry.GetReference(origin) == ObjectRegistry.GetReference(clone);
            #else
            return Cloner.materialMap.ContainsKey(origin) && Cloner.materialMap[origin] == clone;
            #endif
        }

        private static void DresserToChanger(this AutoDresser[] dressers, AutoDresserSettings[] settings, Preset[] presets)
        {
            if(dressers == null || dressers.Length == 0) return;
            CostumeChanger changer;
            if(settings.Length == 1 && settings[0])
            {
                var s = settings[0];
                changer = s.gameObject.AddComponent<CostumeChanger>();
                changer.menuName = s.menuName;
                changer.icon = s.icon;
                changer.parentOverride = s.parentOverride;
                changer.parentOverrideMA = s.parentOverrideMA;
                changer.isSave = s.isSave;
                changer.isLocalOnly = s.isLocalOnly;
                changer.autoFixDuplicate = s.autoFixDuplicate;
                changer.costumes = dressers.DresserToCostumes(out Transform avatarRoot, changer, presets);
                if(changer.costumes == null) Object.DestroyImmediate(changer);
                RegisterReplacedObject(s, changer);
                Object.DestroyImmediate(s);
            }
            else
            {
                var newObj = new GameObject(nameof(AutoDresser));
                changer = newObj.AddComponent<CostumeChanger>();
                changer.menuName = nameof(AutoDresser);
                changer.costumes = dressers.DresserToCostumes(out Transform avatarRoot, changer, presets);
                if(changer.costumes == null) Object.DestroyImmediate(changer);
                newObj.transform.parent = avatarRoot;
                newObj.transform.SetAsFirstSibling();
            }
        }

        // PropをItemTogglerに変換
        private static void PropToToggler(this Prop[] props, Preset[] presets, bool createNewObject = false)
        {
            if(props == null || props.Length == 0) return;
            foreach(var prop in props)
            {
                var obj = prop.gameObject;
                ItemToggler toggler;
                var items = presets.SelectMany(p => p.presetItems).Where(i => i.obj == prop);
                Undo.RecordObjects(presets.Where(p => items.Any(i => p.presetItems.Contains(i))).ToArray(), "Convert to ItemToggler");
                if(createNewObject)
                {
                    obj = new GameObject(prop.GetMenuName());
                    if(prop.parentOverride) obj.transform.parent = prop.parentOverride.transform;
                    else obj.transform.parent = prop.transform.parent;
                    Undo.RegisterCreatedObjectUndo(obj, "Convert to ItemToggler");
                    toggler = Undo.AddComponent<ItemToggler>(obj);
                    EditorGUIUtility.PingObject(obj);
                }
                else
                {
                    toggler = obj.AddComponent<ItemToggler>();
                    RegisterReplacedObject(prop, toggler);
                }
                foreach(var item in items)
                {
                    item.obj = toggler;
                    item.value = 1;
                }
                toggler.menuName = prop.menuName;
                toggler.parentOverride = prop.parentOverride;
                toggler.parentOverrideMA = prop.parentOverrideMA;
                toggler.icon = prop.icon;
                toggler.isSave = prop.isSave;
                toggler.isLocalOnly = prop.isLocalOnly;
                toggler.autoFixDuplicate = prop.autoFixDuplicate;
                toggler.parameter = prop.PropToTogglerParameters();
                if(createNewObject) Undo.DestroyObjectImmediate(prop);
                else Object.DestroyImmediate(prop);
            }
        }

        // 外部から使用する場合は新規オブジェクト作成を強制
        internal static void PropToToggler(Prop[] props, Preset[] presets) => PropToToggler(props, presets, true);

        // MaterialModifierも参照しつつマテリアルから値を取得
        internal static bool TryGetFloat(Material material, string name, out float value)
        {
            if(!material || !material.HasProperty(name))
            {
                value = 0;
                return false;
            }

            var tuple = (material,name);
            if(floatValues.TryGetValue(tuple, out value)) return true; 

            var mods = modifierValues.Where(kv => !kv.Key.ignoreMaterials.Any(i => OriginEquals(i,material)))
                .Where(kv => kv.Value.floatOverride.ContainsKey(name));
            if(!mods.Any()) floatValues[tuple] = value = material.GetFloat(name);
            else floatValues[tuple] = value = mods.Last().Value.floatOverride[name];
            return true;
        }

        internal static bool TryGetVector(Material material, string name, out Vector4 value)
        {
            if(!material || !material.HasProperty(name))
            {
                value = Vector4.zero;
                return false;
            }

            var tuple = (material,name);
            if(vectorValues.TryGetValue(tuple, out value)) return true;

            var mods = modifierValues.Where(kv => !kv.Key.ignoreMaterials.Any(i => OriginEquals(i,material)))
                .Where(kv => kv.Value.vectorOverride.ContainsKey(name));
            if(!mods.Any()) vectorValues[tuple] = value = material.GetVector(name);
            else vectorValues[tuple] = value = mods.Last().Value.vectorOverride[name];
            return true;
        }
    }
}
