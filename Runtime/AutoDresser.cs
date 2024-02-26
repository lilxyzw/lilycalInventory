using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(AutoDresser))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(AutoDresser))]
    internal class AutoDresser : MenuBaseComponent
    {
        [LILLocalizeHeader("inspector.parametersWith", 0)]
        [NotKeyable] [LILBox] public ParametersPerMenu parameter = new ParametersPerMenu();
    }
}
