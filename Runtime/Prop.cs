using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 付けたオブジェクトをオンオフするコンポーネント
    // ビルド時にItemTogglerに変換されます。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(Prop))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "prop")]
    public class Prop : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isSave = true;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isLocalOnly = false;
        [Space(order = 0)] [LILLocalizeHeader("inspector.parametersWith", 1)]
        [NotKeyable] [LILBox] [SerializeField] internal ParametersPerMenu parameter = new ParametersPerMenu();
    }
}
