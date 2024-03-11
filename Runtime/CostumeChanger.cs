using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(CostumeChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(CostumeChanger))]
    internal class CostumeChanger : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] public Costume[] costumes = new Costume[]{};
    }
}
