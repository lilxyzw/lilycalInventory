#if LIL_NDMF
using jp.lilxyzw.avatarmodifier;
using nadena.dev.ndmf;
using nadena.dev.ndmf.builtin;

[assembly: ExportsPlugin(typeof(MaterialModifierPlugin))]

namespace jp.lilxyzw.avatarmodifier
{
    internal class MaterialModifierPlugin : Plugin<MaterialModifierPlugin>
    {
        public override string QualifiedName => "jp.lilxyzw.avatarmodifier";
        public override string DisplayName => "lilMaterialModifier";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).BeforePlugin("com.anatawa12.avatar-optimizer")
            .Run("[lilMaterialModifier] Find and remove components", ctx => Processor.FindComponent(ctx))
            .BeforePass(RemoveEditorOnlyPass.Instance);

            var Transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            Transforming.Run("[lilMaterialModifier] Clone", ctx => Processor.Clone(ctx));
            Transforming.Run("[lilMaterialModifier] Modify", ctx => Processor.Modify(ctx));
            Transforming.Run("[lilMaterialModifier] Remove Component", ctx => Processor.RemoveComponent(ctx));

            InPhase(BuildPhase.Optimizing).Run("[lilMaterialModifier] Optimize", ctx => Processor.Optimize(ctx));
        }
    }
}
#endif
