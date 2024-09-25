using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 無段階でパラメーターを操作するコンポーネント
    // BlendShapeやマテリアルのプロパティ操作を想定しています。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(SmoothChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "smoothchanger")]
    public class SmoothChanger : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isSave = true;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isLocalOnly = false;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool autoFixDuplicate = true;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] [Frame] [SerializeField] internal float defaultFrameValue;
        [NotKeyable] [SerializeField] internal Frame[] frames;

        [System.NonSerialized] internal string parameterName;
    }
}
