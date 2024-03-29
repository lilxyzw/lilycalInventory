using System;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [Serializable]
    internal class Costume : LILElement
    {
        [CostumeName] public string menuName;
        [LILLocalize] public Texture2D icon;
        [LILLocalize] public MenuFolder parentOverride;
        #if LIL_MODULAR_AVATAR
        [LILLocalize] public nadena.dev.modular_avatar.core.ModularAvatarMenuItem parentOverrideMA;
        #else
        [HideInInspector] public UnityEngine.Object parentOverrideMA;
        #endif
        public ParametersPerMenu parametersPerMenu = new ParametersPerMenu();
    }

    [Serializable]
    internal class Frame : LILElement
    {
        [LILLocalize] [Range(0,1)] public float frameValue = 0;
        public ParametersPerMenu parametersPerMenu = new ParametersPerMenu();
    }

    [Serializable]
    internal class ParametersPerMenu
    {
        public ObjectToggler[] objects = new ObjectToggler[]{};
        public BlendShapeModifier[] blendShapeModifiers = new BlendShapeModifier[]{};
        public MaterialReplacer[] materialReplacers = new MaterialReplacer[]{};
        public MaterialPropertyModifier[] materialPropertyModifiers = new MaterialPropertyModifier[]{};
        public const string N_objects = nameof(objects);
        public const string N_blendShapeModifiers = nameof(blendShapeModifiers);
        public const string N_materialReplacers = nameof(materialReplacers);
        public const string N_materialPropertyModifiers = nameof(materialPropertyModifiers);
    }

    [Serializable]
    internal class ObjectToggler
    {
        public GameObject obj = null;
        public bool value = true;
    }

    [Serializable]
    internal class BlendShapeModifier
    {
        [LILLocalize] public SkinnedMeshRenderer skinnedMeshRenderer;
        public BlendShapeNameValue[] blendShapeNameValues;
        [NonSerialized] internal bool applyToAll = false;
    }

    [Serializable]
    internal struct BlendShapeNameValue
    {
        public string name;
        public float value;
    }

    [Serializable]
    internal class MaterialReplacer
    {
        public Renderer renderer;
        public Material[] replaceTo;
    }

    [Serializable]
    internal class MaterialPropertyModifier : LILElementWithoutChildrenFoldout
    {
        public Renderer[] renderers;
        public FloatModifier[] floatModifiers;
        public VectorModifier[] vectorModifiers;
    }

    [Serializable]
    internal struct FloatModifier : LILElementSimple
    {
        [LILLocalize] public string propertyName;
        [LILLocalize] public float value;
    }

    [Serializable]
    internal struct VectorModifier : LILElementSimple
    {
        [LILLocalize] public string propertyName;
        [OneLineVector] public Vector4 value;
        [NoLabel] public bool disableX;
        [NoLabel] public bool disableY;
        [NoLabel] public bool disableZ;
        [NoLabel] public bool disableW;
    }

    interface LILElement {}
    interface LILElementWithoutChildrenFoldout {}
    interface LILElementSimple {}
}
