using UnityEngine;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu("lilAvatarModifier/lilAM ItemToggler")]
    internal class ItemToggler : MenuBaseComponent
    {
        public ParametersPerMenu parameter;
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
