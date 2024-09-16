using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // マテリアルの設定値をセットしたマテリアルに合わせるコンポーネント
    // メニューは生成されません。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(MaterialModifier))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "materialmodifier")]
    public class MaterialModifier : AvatarTagComponent
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal Material referenceMaterial;
        [NotKeyable] [NoLabel] [SerializeField] internal Material[] ignoreMaterials;
        [NotKeyable] [NoLabel] [SerializeField] internal string[] properties;
    }
}
