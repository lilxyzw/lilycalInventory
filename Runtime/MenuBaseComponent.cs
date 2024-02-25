using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [DisallowMultipleComponent]
    internal abstract class MenuBaseComponent : AvatarTagComponent
    {
        [NotKeyable] [MenuName] public string menuName;
        [NotKeyable] [MenuFolderOverride] public MenuFolder parentOverride;
        [NotKeyable] [LILLocalize] public Texture2D icon;
    }
}
