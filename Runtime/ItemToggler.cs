using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // オブジェクトをオンオフするコンポーネント
    // Boolパラメーターが生成されます。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(ItemToggler))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "itemtoggler")]
    public class ItemToggler : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isSave = true;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isLocalOnly = false;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool autoFixDuplicate = true;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] [SerializeField] internal ParametersPerMenu parameter = new ParametersPerMenu();
        [Space(order = 0)] [LILLocalizeHeader("inspector.detailSettings", 1)]
        [NotKeyable] [DefaultValue] [SerializeField] internal bool defaultValue = false;

        [System.NonSerialized] internal string parameterName;
    }

    // こちらは上記と全く同じフィールドを持つクラスです。
    // Componentを直接newするとUnityに怒られるためこちらを経由するようにしています。
    // Propからの変換時に使用されます。
    internal class ItemTogglerInternal
    {
        internal string menuName;
        internal MenuFolder parentOverride;
        internal Texture2D icon;
        #if LIL_MODULAR_AVATAR
        internal nadena.dev.modular_avatar.core.ModularAvatarMenuItem parentOverrideMA;
        #else
        internal Object parentOverrideMA;
        #endif
        internal bool isSave = true;
        internal bool isLocalOnly = false;
        internal bool autoFixDuplicate = true;
        internal ParametersPerMenu parameter = new ParametersPerMenu();
    }
}
