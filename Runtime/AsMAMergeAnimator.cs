using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // LIが生成するAnimatorControllerをMAで統合するコンポーネント
    // MAがない場合は無視される
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(AsMAMergeAnimator))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "asmamergeanimator")]
    public class AsMAMergeAnimator : AvatarTagComponent
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal int layerPriority = 0;
    }
}
