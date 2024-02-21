using UnityEngine;

namespace jp.lilxyzw.materialmodifier.runtime
{
    [AddComponentMenu("lilMaterialModifier/lilMM ItemToggler")]
    internal class ItemToggler : MenuBaseComponent
    {
        public ParametersPerMenu parameter;
        [LILLocalize] public bool isSave = true;
        [LILLocalize] public bool isLocalOnly = false;
    }
}
