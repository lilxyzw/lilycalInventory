using UnityEngine;

namespace jp.lilxyzw.materialmodifier.runtime
{
    [AddComponentMenu("lilMaterialModifier/lilMM CostumeChanger")]
    internal class CostumeChanger : MenuBaseComponent
    {
        public Costume[] costumes;
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
