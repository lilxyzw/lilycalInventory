using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(ItemToggler))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(ItemToggler))]
    internal class ItemToggler : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] public ParametersPerMenu parameter = new ParametersPerMenu();
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
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
        [NotKeyable] public ParametersPerMenu parameter = new ParametersPerMenu();
    }
}
