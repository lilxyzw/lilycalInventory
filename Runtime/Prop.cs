using UnityEngine;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu("lilAvatarModifier/lilAM Prop")]
    internal class Prop : MenuBaseComponent
    {
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
