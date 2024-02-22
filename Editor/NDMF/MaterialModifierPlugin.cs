#if LIL_NDMF
using System.Collections.Generic;
using jp.lilxyzw.materialmodifier;
using jp.lilxyzw.materialmodifier.runtime;
using nadena.dev.ndmf;
using nadena.dev.ndmf.builtin;
using UnityEngine;

[assembly: ExportsPlugin(typeof(MaterialModifierPlugin))]

namespace jp.lilxyzw.materialmodifier
{
    internal class MaterialModifierPlugin : Plugin<MaterialModifierPlugin>
    {
        // Common
        private static bool shouldModify;
        // Material
        private static MaterialModifier[] modifiers;
        private static MaterialOptimizer[] optimizers;
        // Menu
        private static MenuFolder[] folders;
        private static ItemToggler[] togglers;
        private static Prop[] props;
        private static CostumeChanger[] costumeChangers;
        private static SmoothChanger[] smoothChangers;
        private static Material[] materials;

        public override string QualifiedName => "jp.lilxyzw.materialmodifier";
        public override string DisplayName => "lilMaterialModifier";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).BeforePlugin("com.anatawa12.avatar-optimizer")
            .Run("[lilMaterialModifier] Find and remove components", ctx => {
                var components = ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>();
                modifiers = components.GetActiveComponents<MaterialModifier>();
                optimizers = components.GetActiveComponents<MaterialOptimizer>();
                folders = components.GetActiveComponents<MenuFolder>();
                togglers = components.GetActiveComponents<ItemToggler>();
                props = components.GetActiveComponents<Prop>();
                costumeChangers = components.GetActiveComponents<CostumeChanger>();
                smoothChangers = components.GetActiveComponents<SmoothChanger>();
                shouldModify = components.Length != 0;
                if(!shouldModify) return;
                components.GetActiveComponents<MenuBaseComponent>().ResolveMenuName();
                components.GetActiveComponents<CostumeChanger>().ResolveMenuName();
                ObjHelper.CheckApplyToAll(togglers, costumeChangers, smoothChangers);
            }).BeforePass(RemoveEditorOnlyPass.Instance);

            var Transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            Transforming.Run("[lilMaterialModifier] Clone", ctx => {
                if(!shouldModify) return;
                materials = Cloner.DeepCloneAssets(ctx);
            });

            Transforming.Run("[lilMaterialModifier] Modify", ctx => {
                if(!shouldModify) return;
                #if LIL_VRCSDK3A
                var controller = ctx.AvatarDescriptor.TryGetFXAnimatorController(ctx);
                var hasWriteDefaultsState = controller.HasWriteDefaultsState();
                var menu = VRChatHelper.CreateMenu(ctx);
                var parameters = VRChatHelper.CreateParameters(ctx);
                var menuDic = new Dictionary<MenuFolder, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
                Modifier.ApplyMenuFolder(ctx, folders, menu, menuDic);
                Modifier.ApplyItemToggler(ctx, controller, hasWriteDefaultsState, togglers, menu, parameters, menuDic);
                Modifier.ApplyProp(ctx, controller, hasWriteDefaultsState, props, menu, parameters, menuDic);
                Modifier.ApplyCostumeChanger(ctx, controller, hasWriteDefaultsState, costumeChangers, menu, parameters, menuDic);
                Modifier.ApplySmoothChanger(ctx, controller, hasWriteDefaultsState, smoothChangers, menu, parameters, menuDic);
                ctx.AvatarDescriptor.MergeParameters(menu, parameters);
                #else
                // Not supported
                #endif
                Modifier.ApplyMaterialModifier(materials, modifiers);
            });

            Transforming.Run("[lilMaterialModifier] Remove Component", ctx => {
                if(!shouldModify) return;
                foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true))
                    Object.DestroyImmediate(component);
            });

            InPhase(BuildPhase.Optimizing).Run("[lilMaterialModifier] Optimize", ctx => {
                if(shouldModify && optimizers.Length != 0) Optimizer.OptimizeMaterials(materials);
            });
        }
    }
}
#endif
