using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 各メニュー生成コンポーネントのベースとなるクラス
    // メニューに必須なプロパティはここにまとめています
    public abstract class MenuBaseComponent : AvatarTagComponent
    {
        [NotKeyable] [MenuName] [SerializeField] internal string menuName;
        [NotKeyable] [MenuFolderOverride] [SerializeField] internal MenuFolder parentOverride;
        [NotKeyable] [LILLocalize] [SerializeField] internal Texture2D icon;
        #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
        [NotKeyable] [LILLocalize] [SerializeField] internal nadena.dev.modular_avatar.core.ModularAvatarMenuItem parentOverrideMA;
        #else
        [NotKeyable] [SerializeField] internal Object parentOverrideMA;
        #endif
    }

    [DisallowMultipleComponent]
    public abstract class MenuBaseDisallowMultipleComponent : MenuBaseComponent
    {
    }
}
