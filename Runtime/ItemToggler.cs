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
}
