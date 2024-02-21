using UnityEngine;

namespace jp.lilxyzw.materialmodifier.runtime
{
    [AddComponentMenu("lilMaterialModifier/lilMM SmoothChanger")]
    internal class SmoothChanger : MenuBaseComponent
    {
        public Frame[] frame;
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
