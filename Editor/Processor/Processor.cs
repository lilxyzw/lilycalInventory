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
        // Common
        private static bool shouldModify;
        // Material
        private static MaterialModifier[] modifiers;
        private static MaterialOptimizer[] optimizers;
        // Menu
        private static AvatarTagComponent[] components;
        private static AutoDresserSettings[] dresserSettings;
        private static AutoDresser[] dressers;
        private static Prop[] props;
        private static MenuFolder[] folders;
        private static ItemToggler[] togglers;
        private static CostumeChanger[] costumeChangers;
        private static SmoothChanger[] smoothChangers;
        private static Material[] materials;
        private static HashSet<string> parameterNames;
        private static (GameObject,bool)[] actives;

        internal static void FindComponent(BuildContext ctx)
        {
            Cloner.materialMap = new Dictionary<Material, Material>();

            // Remove Comment Component
            foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<Comment>(true))
                Object.DestroyImmediate(component);

            dressers = ctx.AvatarRootObject.GetActiveComponentsInChildren<AutoDresser>(true);
            props = ctx.AvatarRootObject.GetActiveComponentsInChildren<Prop>(true);
            actives = dressers.Select(d => (d.gameObject,d.gameObject.activeSelf)).Union(props.Select(p => (p.gameObject,p.gameObject.activeSelf))).ToArray();

            // Resolve Dresser
            dresserSettings = ctx.AvatarRootObject.GetActiveComponentsInChildren<AutoDresserSettings>(false);
            if(dresserSettings.Length > 1) ErrorHelper.Report("dialog.error.dresserSettingsDuplicate", dresserSettings);
            dressers.ResolveMenuName();
            dressers.DresserToChanger(dresserSettings);
            // Resolve Prop
            props.ResolveMenuName();
            props.PropToToggler();

            // Set force active recursive
            void ForceActive(MenuBaseComponent component)
            {
                var parent = component.GetMenuParent();
                if(!parent) return;
                parent.forceActive = true;
                ForceActive(parent);
            }

            foreach(var component in ctx.AvatarRootObject.GetActiveComponentsInChildren<MenuBaseComponent>(true).Where(c => c.forceActive).ToArray())
                ForceActive(component);

            foreach(var a in actives) a.Item1.SetActive(true);
            components = ctx.AvatarRootObject.GetActiveComponentsInChildren<AvatarTagComponent>(true).Where(c => c.gameObject.activeInHierarchy || c.forceActive).ToArray();
            foreach(var a in actives) a.Item1.SetActive(a.Item2);
            modifiers = components.SelectComponents<MaterialModifier>();
            optimizers = components.SelectComponents<MaterialOptimizer>();
            folders = components.SelectComponents<MenuFolder>();
            togglers = components.SelectComponents<ItemToggler>();
            costumeChangers = components.SelectComponents<CostumeChanger>();
            smoothChangers = components.SelectComponents<SmoothChanger>();
            shouldModify = components.Length != 0;
            if(!shouldModify) return;
            components.SelectComponents<MenuBaseComponent>().ResolveMenuName();
            components.SelectComponents<CostumeChanger>().ResolveMenuName();
            ObjHelper.CheckApplyToAll(togglers, costumeChangers, smoothChangers);
            ObjHelper.CheckUseless(togglers);

            ModularAvatarHelper.ResolveMenu(folders, togglers, costumeChangers, smoothChangers);
        }

        internal static void CloneAssets(BuildContext ctx)
        {
            if(!shouldModify) return;
            Cloner.DeepCloneAssets(ctx);
        }

        internal static void ModifyPreProcess(BuildContext ctx)
        {
            if(!shouldModify) return;
            #if LIL_VRCSDK3A
            if(togglers.Length + costumeChangers.Length + smoothChangers.Length > 0)
            {
                var controller = ctx.AvatarDescriptor.TryGetFXAnimatorController(ctx);
                var hasWriteDefaultsState = controller.HasWriteDefaultsState();
                var menu = VRChatHelper.CreateMenu(ctx);
                var parameters = VRChatHelper.CreateParameters(ctx);
                var menuDic = new Dictionary<MenuFolder, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();

                parameterNames = new HashSet<string>();
                if(controller.parameters != null) parameterNames.UnionWith(controller.parameters.Select(p => p.name));
                if(ctx.AvatarDescriptor.expressionParameters) ctx.AvatarDescriptor.expressionParameters.parameters.Select(p => p.name);
                var parameterDuplicates = components.SelectComponents<MenuBaseComponent>().Where(c => c is IGenerateParameter && parameterNames.Contains(c.menuName)).Select(c => c.gameObject).ToArray();
                if(parameterDuplicates.Length > 0) ErrorHelper.Report("dialog.error.parameterDuplication", parameterDuplicates);

                BlendTree tree = null;
                if(ToolSettings.instance.useDirectBlendTree)
                {
                    AnimationHelper.CreateLayer(controller, out tree);
                    AssetDatabase.AddObjectToAsset(tree, ctx.AssetContainer);
                }
                Modifier.ResolveMultiConditions(ctx, controller, hasWriteDefaultsState, togglers, costumeChangers, tree);
                Modifier.ApplyItemToggler(ctx, controller, hasWriteDefaultsState, togglers, tree, parameters);
                Modifier.ApplyCostumeChanger(ctx, controller, hasWriteDefaultsState, costumeChangers, tree, parameters);
                Modifier.ApplySmoothChanger(ctx, controller, hasWriteDefaultsState, smoothChangers, tree, parameters);
                if(ToolSettings.instance.useDirectBlendTree) AnimationHelper.SetParameter(tree);
                MenuGenerator.Generate(ctx, folders, togglers, smoothChangers, costumeChangers, menu, menuDic);
                ctx.AvatarDescriptor.MergeParameters(menu, parameters, ctx);
            }
            #else
            // Not supported
            #endif
        }

        internal static void CloneMaterials(BuildContext ctx)
        {
            if(!shouldModify) return;
            materials = Cloner.CloneAllMaterials(ctx);
        }

        internal static void ModifyPostProcess(BuildContext ctx)
        {
            if(!shouldModify) return;
            Modifier.ApplyMaterialModifier(materials, modifiers);
        }

        internal static void RemoveComponent(BuildContext ctx)
        {
            foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true))
                Object.DestroyImmediate(component);
        }

        internal static void Optimize(BuildContext ctx)
        {
            if(shouldModify && optimizers.Length != 0) Optimizer.OptimizeMaterials(materials);
        }
    }
}
