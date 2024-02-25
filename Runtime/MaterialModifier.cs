using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(MaterialModifier))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(MaterialModifier))]
    internal class MaterialModifier : AvatarTagComponent
    {
        [NotKeyable] [LILLocalize] public Material referenceMaterial;
        [NotKeyable] [NoLabel] public Material[] ignoreMaterials;
        [NotKeyable] [NoLabel] public string[] properties;
    }
}
