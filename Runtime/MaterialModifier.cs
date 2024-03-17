using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // マテリアルの設定値をセットしたマテリアルに合わせるコンポーネント
    // メニューは生成されません。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(MaterialModifier))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(MaterialModifier))]
    internal class MaterialModifier : AvatarTagComponent
    {
        [NotKeyable] [LILLocalize] public Material referenceMaterial;
        [NotKeyable] [NoLabel] public Material[] ignoreMaterials;
        [NotKeyable] [NoLabel] public string[] properties;
    }
}
