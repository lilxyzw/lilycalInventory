using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu("lilAvatarModifier/lilAM MaterialModifier")]
    internal class MaterialModifier : AvatarTagComponent
    {
        [LILLocalize] [NotKeyable] public Material referenceMaterial;
        [NoLabel] [NotKeyable] public Material[] ignoreMaterials;
        [NoLabel] [NotKeyable] public string[] properties;
    }
}
