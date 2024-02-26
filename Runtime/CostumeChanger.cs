using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(CostumeChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(CostumeChanger))]
    internal class CostumeChanger : MenuBaseComponent
    {
        [NotKeyable] public Costume[] costumes = new Costume[]{};
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
    }
}
