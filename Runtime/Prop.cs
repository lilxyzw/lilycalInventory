using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 付けたオブジェクトをオンオフするコンポーネント
    // ビルド時にItemTogglerに変換されます。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(Prop))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "prop")]
    internal class Prop : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
        [Space(order = 0)] [LILLocalizeHeader("inspector.parametersWith", 1)]
        [NotKeyable] [LILBox] public ParametersPerMenu parameter = new ParametersPerMenu();
    }
}
