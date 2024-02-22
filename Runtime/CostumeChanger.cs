using UnityEngine;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu("lilAvatarModifier/lilAM CostumeChanger")]
    internal class CostumeChanger : MenuBaseComponent
    {
        public Costume[] costumes;
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
