using UnityEngine;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    internal abstract class MenuBaseComponent : AvatarTagComponent
    {
        [MenuName] public string menuName;
        [MenuFolderOverride] public MenuFolder parentOverride;
        [LILLocalize] public Texture2D icon;
    }
}
