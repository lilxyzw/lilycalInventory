using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // AutoDresserの設定用コンポーネント
    // 1アバターごとに1つまで設定でき、ここの設定をもとにAutoDresser変換後のCostumeChangerの設定が行われます。
    [DisallowMultipleComponent]
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(AutoDresserSettings))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "autodressersettings")]
    public class AutoDresserSettings : MenuBaseComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isSave = true;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isLocalOnly = false;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool autoFixDuplicate = true;
    }
}
