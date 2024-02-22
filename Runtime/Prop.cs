using UnityEngine;

namespace jp.lilxyzw.materialmodifier.runtime
{
    [AddComponentMenu("lilMaterialModifier/lilMM Prop")]
    internal class Prop : MenuBaseComponent
    {
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
