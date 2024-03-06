using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(AutoDresserSettings))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(AutoDresserSettings))]
    internal class AutoDresserSettings : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
    }
}
