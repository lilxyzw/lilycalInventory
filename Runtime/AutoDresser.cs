using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(AutoDresser))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(AutoDresser))]
    internal class AutoDresser : MenuBaseDisallowMultipleComponent
    {
        [LILLocalizeHeader("inspector.parametersWith", 0)]
        [NotKeyable] [LILBox] public ParametersPerMenu parameter = new ParametersPerMenu();
    }
}
