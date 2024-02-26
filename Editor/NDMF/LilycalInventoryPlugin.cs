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
            .Run($"[{ConstantValues.TOOL_NAME}] Find and remove components", ctx => Processor.FindComponent(ctx))
            .BeforePass(RemoveEditorOnlyPass.Instance);

            var Transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            Transforming.Run($"[{ConstantValues.TOOL_NAME}] Clone", ctx => Processor.Clone(ctx));
            Transforming.Run($"[{ConstantValues.TOOL_NAME}] Modify", ctx => Processor.Modify(ctx));
            Transforming.Run($"[{ConstantValues.TOOL_NAME}] Remove Component", ctx => Processor.RemoveComponent(ctx));

            InPhase(BuildPhase.Optimizing).Run($"[{ConstantValues.TOOL_NAME}] Optimize", ctx => Processor.Optimize(ctx));
        }
    }
}
#endif
