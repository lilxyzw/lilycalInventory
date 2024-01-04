#if LIL_NDMF
using System.Linq;
using jp.lilxyzw.materialmodifier;
using jp.lilxyzw.materialmodifier.runtime;
using nadena.dev.ndmf;
using nadena.dev.ndmf.builtin;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(MaterialModifierPlugin))]

namespace jp.lilxyzw.materialmodifier
{
    internal class MaterialModifierPlugin : Plugin<MaterialModifierPlugin>
    {
        private static MaterialModifier[] modifiers;
        private static MaterialOptimizer[] optimizers;
        private static Material[] materials;
        private static bool shouldModify;

        public override string QualifiedName => "jp.lilxyzw.materialmodifier";
        public override string DisplayName => "lilMaterialModifier";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).BeforePlugin("com.anatawa12.avatar-optimizer")
            .Run("[lilMaterialModifier] Find and remove components", ctx => {
                var components = ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>();
                modifiers = components.Select(c => c as MaterialModifier).Where(c => c != null).ToArray();
                optimizers = components.Select(c => c as MaterialOptimizer).Where(c => c != null).ToArray();
                shouldModify = components.Length != 0;
                foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true))
                    Object.DestroyImmediate(component);
            }).BeforePass(RemoveEditorOnlyPass.Instance);

            var Transforming = InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar");
            Transforming.Run("[lilMaterialModifier] Clone", ctx => {
                if(shouldModify) materials = Cloner.DeepCloneMaterials(ctx);
            });

            Transforming.Run("[lilMaterialModifier] Modify", ctx => {
                if(shouldModify && modifiers.Length != 0) Modifier.ModifyMaterials(materials, modifiers);
            });

            InPhase(BuildPhase.Optimizing).Run("[lilMaterialModifier] Optimize", ctx => {
                if(shouldModify && optimizers.Length != 0) Optimizer.OptimizeMaterials(materials);
            });
        }
    }
}
#endif
