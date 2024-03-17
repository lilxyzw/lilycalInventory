using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // マテリアルから不要なプロパティを除去して最適化するコンポーネント
    // メニューは生成されません。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(MaterialOptimizer))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(MaterialOptimizer))]
    internal class MaterialOptimizer : AvatarTagComponent
    {
    }
}
