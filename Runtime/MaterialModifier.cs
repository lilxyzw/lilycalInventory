using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.materialmodifier.runtime
{
    [AddComponentMenu("lilMaterialModifier/lilMM Modifier")]
    internal class MaterialModifier : AvatarTagComponent
    {
        [LILLocalize] [NotKeyable] public Material referenceMaterial;
        [NoLabel] [NotKeyable] public Material[] ignoreMaterials;
        [NoLabel] [NotKeyable] public string[] properties;
    }
}
