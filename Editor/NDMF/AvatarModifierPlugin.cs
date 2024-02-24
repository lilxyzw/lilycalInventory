#if LIL_NDMF
using jp.lilxyzw.avatarmodifier;
using nadena.dev.ndmf;
using nadena.dev.ndmf.builtin;

[assembly: ExportsPlugin(typeof(AvatarModifierPlugin))]

namespace jp.lilxyzw.avatarmodifier
{
    internal class AvatarModifierPlugin : Plugin<AvatarModifierPlugin>
    {
        public override string QualifiedName => "jp.lilxyzw.avatarmodifier";
        public override string DisplayName => "lilAvatarModifier";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).BeforePlugin("com.anatawa12.avatar-optimizer")
            .Run("[lilAvatarModifier] Find and remove components", ctx => Processor.FindComponent(ctx))
            .BeforePass(RemoveEditorOnlyPass.Instance);

            var Transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            Transforming.Run("[lilAvatarModifier] Clone", ctx => Processor.Clone(ctx));
            Transforming.Run("[lilAvatarModifier] Modify", ctx => Processor.Modify(ctx));
            Transforming.Run("[lilAvatarModifier] Remove Component", ctx => Processor.RemoveComponent(ctx));

            InPhase(BuildPhase.Optimizing).Run("[lilAvatarModifier] Optimize", ctx => Processor.Optimize(ctx));
        }
    }
}
#endif
