using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(ItemToggler))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(ItemToggler))]
    internal class ItemToggler : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] public ParametersPerMenu parameter = new ParametersPerMenu();
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
    }

    internal class ItemTogglerInternal
    {
        [NotKeyable] [MenuName] public string menuName;
        [NotKeyable] [MenuFolderOverride] public MenuFolder parentOverride;
        [NotKeyable] [LILLocalize] public Texture2D icon;
        #if LIL_MODULAR_AVATAR
        [NotKeyable] [LILLocalize] public nadena.dev.modular_avatar.core.ModularAvatarMenuItem parentOverrideMA;
        #else
        [NotKeyable] [HideInInspector] public Object parentOverrideMA;
        #endif
        [NotKeyable] public ParametersPerMenu parameter = new ParametersPerMenu();
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
    }
}
