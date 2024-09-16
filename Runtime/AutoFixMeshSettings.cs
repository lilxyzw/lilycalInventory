using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // メッシュ設定を自動統一するコンポーネント
    // メニューは生成されません。
    [DisallowMultipleComponent]
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(AutoFixMeshSettings))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "autofixmeshsettings")]
    public class AutoFixMeshSettings : AvatarTagComponent
    {
        [NotKeyable] [NoLabel] [SerializeField] internal Renderer[] ignoreRenderers;
        [LILLocalize] [SerializeField] internal MeshSettings meshSettings;
    }

    [Serializable]
    internal class MeshSettings
    {
        [NotKeyable] public bool updateWhenOffscreen = false;
        [NotKeyable] public Transform rootBone;
        [NotKeyable] [LILLocalize] public bool autoCalculateBounds = true;
        [NotKeyable] [LILDisableWhen("meshSettings.autoCalculateBounds", true)] public Bounds bounds;
        [NotKeyable] public ShadowCastingMode castShadows = ShadowCastingMode.On;
        [NotKeyable] public bool receiveShadows = true;
        [NotKeyable] public LightProbeUsage lightProbes = LightProbeUsage.BlendProbes;
        [NotKeyable] public ReflectionProbeUsage reflectionProbes = ReflectionProbeUsage.BlendProbes;
        [NotKeyable] public Transform anchorOverride;
        [NotKeyable] public MotionVectorGenerationMode motionVectors = MotionVectorGenerationMode.Object;
        [NotKeyable] public bool dynamicOcclusion = true;
        [NotKeyable] public bool skinnedMotionVectors = true;
    }
}
