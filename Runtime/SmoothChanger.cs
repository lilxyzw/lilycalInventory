using UnityEngine;

namespace jp.lilxyzw.materialmodifier.runtime
{
    [AddComponentMenu("lilMaterialModifier/lilMM SmoothChanger")]
    internal class SmoothChanger : MenuBaseComponent
    {
        [LILLocalize] [Range(0,1)] public float defaultFrameValue;
        public Frame[] frames;
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
