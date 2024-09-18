#if LIL_NDMF
using jp.lilxyzw.lilycalinventory;
using jp.lilxyzw.lilycalinventory.runtime;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(LilycalInventoryPlugin))]

namespace jp.lilxyzw.lilycalinventory
{
    internal class LilycalInventoryPlugin : Plugin<LilycalInventoryPlugin>
    {
        public static Texture2D m_Logo;
        public override string QualifiedName => ConstantValues.PACKAGE_NAME_FULL;
        public override string DisplayName => ConstantValues.TOOL_NAME;

        // ロゴの色と同じ
        public override Texture2D LogoTexture => m_Logo ? m_Logo : m_Logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("c174ba1806ba8004db0ed2fa0832479d"));
        public override Color? ThemeColor => new Color(0.572f, 0.549f, 0.858f);

        protected override void Configure()
        {
            // MenuItemを生成する関係でMAの前に実行
            var Resolving = InPhase(BuildPhase.Resolving).BeforePlugin("nadena.dev.modular-avatar");
            Resolving.Run("Find LI Components", ctx => Processor.FindComponent(ctx));

            // アニメーションの生成はMAの前に行う
            var Transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            Transforming.Run("ModifyPreProcess", ctx => Processor.ModifyPreProcess(ctx));

            // マテリアルのクローンおよび改変はMAやTTTの後に行う
            var TransformingPostProcess = InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar").AfterPlugin("net.rs64.tex-trans-tool");
            TransformingPostProcess.Run("ModifyPostProcess", ctx => Processor.ModifyPostProcess(ctx));
            TransformingPostProcess.Run("Remove Component", ctx => Processor.RemoveComponent(ctx));

            // 現状ではいつ実行してもいい
            InPhase(BuildPhase.Optimizing).Run("Optimize", ctx => Processor.Optimize(ctx));
        }
    }
}
#endif
