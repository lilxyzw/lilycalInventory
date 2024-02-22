using jp.lilxyzw.avatarmodifier.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
{
    internal static partial class AnimationHelper
    {
        internal static (AnimationClip,AnimationClip) CreateClip(this Prop prop, string name)
        {
            var clipOff = new AnimationClip();
            var clipOn = new AnimationClip();
            clipOff.name = $"{name}_Off";
            clipOn.name = $"{name}_On";

            var obj = prop.gameObject;

            var binding = CreateToggleBinding(obj);
            var curveOff = SimpleCurve(obj.activeSelf);
            AnimationUtility.SetEditorCurve(clipOff, binding, curveOff);
            var curveOn = SimpleCurve(!obj.activeSelf);
            AnimationUtility.SetEditorCurve(clipOn, binding, curveOn);

            return (clipOff, clipOn);
        }
    }
}
