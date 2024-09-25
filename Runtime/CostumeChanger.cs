using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 排他的に切り替えるオブジェクト（衣装など）を設定するコンポーネント
    // Intパラメーターが生成されます。
    // Boolに置き換えてパラメーター数を節約するのもありかもしれません。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(CostumeChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "costumechanger")]
    public class CostumeChanger : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isSave = true;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool isLocalOnly = false;
        [NotKeyable] [LILLocalize] [SerializeField] internal bool autoFixDuplicate = true;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] [SerializeField] internal Costume[] costumes = new Costume[]{};
        [Space(order = 0)] [LILLocalizeHeader("inspector.detailSettings", 1)]
        [NotKeyable] [DefaultValue("costumes")] [SerializeField] internal int defaultValue = 0;

        [System.NonSerialized] internal string parameterName;
        [System.NonSerialized] internal string parameterNameLocal;
        [System.NonSerialized] internal string[] parameterNameBits;
    }
}
