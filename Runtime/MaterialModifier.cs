using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.materialmodifier.runtime
{
    [AddComponentMenu("lilMaterialModifier/lilMM Modifier")]
    internal class MaterialModifier : AvatarTagComponent
    {
        [NotKeyable] public Material referenceMaterial;
        [NotKeyable] public Material[] ignoreMaterials;
        [NotKeyable] public string[] properties;
    }
}
