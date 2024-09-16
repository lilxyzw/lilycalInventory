using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 着せ替え用コンポーネント
    // アバター内のAutoDresserを検索して、そこからCostumeChangerに変換されます。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(AutoDresser))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "autodresser")]
    public class AutoDresser : MenuBaseDisallowMultipleComponent
    {
        [Space(order = 0)] [LILLocalizeHeader("inspector.parametersWith", 1)]
        [NotKeyable] [LILBox] [SerializeField] internal ParametersPerMenu parameter = new ParametersPerMenu();
    }
}
