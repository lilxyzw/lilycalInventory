using System;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier.runtime
{
    [Serializable]
    internal class Costume : LILElement
    {
        [CostumeName] public string menuName;
        [LILLocalize] public Texture2D icon;
        public ParametersPerMenu parametersPerMenu;
    }

    [Serializable]
    internal class Frame : LILElement
    {
        [LILLocalize] [Range(0,1)] public float frameValue;
        public ParametersPerMenu parametersPerMenu;
    }

    [Serializable]
    internal class ParametersPerMenu
    {
        public ObjectToggler[] objects;
        public BlendShapeModifier[] blendShapeModifiers;
        public MaterialReplacer[] materialReplacers;
        public MaterialPropertyModifier[] materialPropertyModifiers;
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
    internal class BlendShapeNameValue
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
    internal class MaterialPropertyModifier : LILElement
    {
        public Renderer[] renderers;
        public FloatModifier[] floatModifiers;
        public VectorModifier[] vectorModifiers;
    }

    [Serializable]
    internal class FloatModifier : LILElementSimple
    {
        [LILLocalize] public string propertyName;
        [LILLocalize] public float value;
    }

    [Serializable]
    internal class VectorModifier : LILElementSimple
    {
        [LILLocalize] public string propertyName;
        [OneLineVector] public Vector4 value;
    }

    [Serializable]
    abstract class LILElement {}

    [Serializable]
    abstract class LILElementSimple {}
}
