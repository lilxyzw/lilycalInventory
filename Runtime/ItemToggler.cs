using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(ItemToggler))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(ItemToggler))]
    internal class ItemToggler : MenuBaseComponent
    {
        [NotKeyable] public ParametersPerMenu parameter;
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
    }
}
