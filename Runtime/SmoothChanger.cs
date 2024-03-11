using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(SmoothChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(SmoothChanger))]
    internal class SmoothChanger : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [Frame] public float defaultFrameValue;
        [NotKeyable] public Frame[] frames;
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
    }
}
