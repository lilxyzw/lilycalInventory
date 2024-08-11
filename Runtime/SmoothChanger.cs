using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 無段階でパラメーターを操作するコンポーネント
    // BlendShapeやマテリアルのプロパティ操作を想定しています。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(SmoothChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "smoothchanger")]
    internal class SmoothChanger : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] [Frame] public float defaultFrameValue;
        [NotKeyable] public Frame[] frames;
    }
}
