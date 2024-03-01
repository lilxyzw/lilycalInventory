#if LIL_NDMF
using jp.lilxyzw.lilycalinventory;
using jp.lilxyzw.lilycalinventory.runtime;
using nadena.dev.ndmf;
using nadena.dev.ndmf.builtin;

[assembly: ExportsPlugin(typeof(LilycalInventoryPlugin))]

namespace jp.lilxyzw.lilycalinventory
{
    internal class LilycalInventoryPlugin : Plugin<LilycalInventoryPlugin>
    {
        public override string QualifiedName => ConstantValues.PACKAGE_NAME_FULL;
        public override string DisplayName => ConstantValues.TOOL_NAME;

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).BeforePlugin("com.anatawa12.avatar-optimizer")
            .Run("Find components", ctx => Processor.FindComponent(ctx))
            .BeforePass(RemoveEditorOnlyPass.Instance);

            var Transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            Transforming.Run("Clone", ctx => Processor.Clone(ctx));
            Transforming.Run("ModifyPreProcess", ctx => Processor.ModifyPreProcess(ctx));

            var TransformingPostProcess = InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar");
            TransformingPostProcess.Run("ClonePostProcess", ctx => Processor.Clone(ctx));
            TransformingPostProcess.Run("ModifyPostProcess", ctx => Processor.ModifyPostProcess(ctx));
            TransformingPostProcess.Run("Remove Component", ctx => Processor.RemoveComponent(ctx));

            InPhase(BuildPhase.Optimizing).Run("Optimize", ctx => Processor.Optimize(ctx));
        }
    }
}
#endif
