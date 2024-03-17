#if !LIL_NDMF
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    // NDMFがない場合のダミー
    [PreferBinarySerialization]
    internal class AssetContainer : ScriptableObject { }
}
#endif
