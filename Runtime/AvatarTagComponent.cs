using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // lilycalInventoryで追加するコンポーネントは全てこれを継承してください。
    // アバタービルド時に自動で削除する処理が実行されます。
    public abstract class AvatarTagComponent : MonoBehaviour
    #if LIL_VRCSDK3
    , VRC.SDKBase.IEditorOnly
    #endif
    {
        void Start(){}
    }
}
