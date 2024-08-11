using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // オブジェクトをオンオフするコンポーネント
    // Boolパラメーターが生成されます。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(ItemToggler))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "itemtoggler")]
    internal class ItemToggler : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] public ParametersPerMenu parameter = new ParametersPerMenu();
        [Space(order = 0)] [LILLocalizeHeader("inspector.detailSettings", 1)]
        [NotKeyable] [DefaultValue] public bool defaultValue = false;
    }

    // こちらは上記と全く同じフィールドを持つクラスです。
    // Componentを直接newするとUnityに怒られるためこちらを経由するようにしています。
    // Propからの変換時に使用されます。
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
