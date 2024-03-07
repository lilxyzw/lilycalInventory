#if LIL_NDMF
using jp.lilxyzw.lilycalinventory;
using jp.lilxyzw.lilycalinventory.runtime;
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(LilycalInventoryPlugin))]

namespace jp.lilxyzw.lilycalinventory
{
    internal class LilycalInventoryPlugin : Plugin<LilycalInventoryPlugin>
    {
        public override string QualifiedName => ConstantValues.PACKAGE_NAME_FULL;
        public override string DisplayName => ConstantValues.TOOL_NAME;

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).BeforePlugin("nadena.dev.modular-avatar")
            .Run("Find LI Components", ctx => Processor.FindComponent(ctx));

            var Transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            Transforming.Run("Clone Assets", ctx => Processor.CloneAssets(ctx));
            Transforming.Run("ModifyPreProcess", ctx => Processor.ModifyPreProcess(ctx));

            var TransformingPostProcess = InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar").AfterPlugin("net.rs64.tex-trans-tool");
            TransformingPostProcess.Run("Clone Materials", ctx => Processor.CloneMaterials(ctx));
            TransformingPostProcess.Run("ModifyPostProcess", ctx => Processor.ModifyPostProcess(ctx));
            TransformingPostProcess.Run("Remove Component", ctx => Processor.RemoveComponent(ctx));

            InPhase(BuildPhase.Optimizing).Run("Optimize", ctx => Processor.Optimize(ctx));
        }
    }
}
#endif
