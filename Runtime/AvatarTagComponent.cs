using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    internal abstract class AvatarTagComponent : MonoBehaviour
    #if LIL_VRCSDK3
    , VRC.SDKBase.IEditorOnly
    #endif
    {
        void Start(){}
    }
}
