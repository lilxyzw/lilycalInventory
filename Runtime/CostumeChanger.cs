using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 排他的に切り替えるオブジェクト（衣装など）を設定するコンポーネント
    // Intパラメーターが生成されます。
    // Boolに置き換えてパラメーター数を節約するのもありかもしれません。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(CostumeChanger))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(CostumeChanger))]
    internal class CostumeChanger : MenuBaseDisallowMultipleComponent, IGenerateParameter
    {
        [NotKeyable] [LILLocalize] public bool isSave = true;
        [NotKeyable] [LILLocalize] public bool isLocalOnly = false;
        [Space(order = 0)] [LILLocalizeHeader("inspector.animationSettings", 1)]
        [NotKeyable] public Costume[] costumes = new Costume[]{};
        [Space(order = 0)] [LILLocalizeHeader("inspector.detailSettings", 1)]
        [NotKeyable] [DefaultValue("costumes")] public int defaultValue = 0;
    }
}
