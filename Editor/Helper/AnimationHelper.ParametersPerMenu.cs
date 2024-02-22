using jp.lilxyzw.materialmodifier.runtime;
using UnityEditor;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.materialmodifier
{
    internal static partial class AnimationHelper
    {
        internal static (AnimationClip,AnimationClip) CreateClip(this ParametersPerMenu parameter, BuildContext ctx, string name)
        {
            var clipOff = new AnimationClip();
            var clipOn = new AnimationClip();
            clipOff.name = $"{name}_Off";
            clipOn.name = $"{name}_On";

            foreach(var toggler in parameter.objects)
            {
                if(!toggler.obj) continue;
                toggler.ToClipDefault(clipOff);
                toggler.ToClip(clipOn);
            }

            foreach(var modifier in parameter.blendShapeModifiers)
            {
                if(modifier.applyToAll)
                {
                    var renderers = ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach(var renderer in renderers)
                    {
                        if(!renderer || !renderer.sharedMesh) continue;
                        foreach(var namevalue in modifier.blendShapeNameValues)
                        {
                            if(renderer.sharedMesh.GetBlendShapeIndex(namevalue.name) == -1) continue;
                            namevalue.ToClipDefault(clipOff, renderer);
                            namevalue.ToClip(clipOn, renderer);
                        }
                    }
                    continue;
                }
                if(!modifier.skinnedMeshRenderer) continue;
                foreach(var namevalue in modifier.blendShapeNameValues)
                {
                    namevalue.ToClipDefault(clipOff, modifier.skinnedMeshRenderer);
                    namevalue.ToClip(clipOn, modifier.skinnedMeshRenderer);
                }
            }

            foreach(var replacer in parameter.materialReplacers)
            {
                if(!replacer.renderer) continue;
                replacer.ToClipDefault(clipOff);
                replacer.ToClip(clipOn);
            }

            foreach(var modifier in parameter.materialPropertyModifiers)
            {
                modifier.ToClipDefault(clipOff);
                modifier.ToClip(clipOn);
            }
            return (clipOff, clipOn);
        }

        // ObjectToggler
        private static void ToClipDefault(this ObjectToggler toggler, AnimationClip clip)
        {
            var binding = CreateToggleBinding(toggler.obj);
            var curve = SimpleCurve(!toggler.value);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            toggler.obj.SetActive(!toggler.value);
        }

        private static void ToClip(this ObjectToggler toggler, AnimationClip clip)
        {
            var binding = CreateToggleBinding(toggler.obj);
            var curve = SimpleCurve(toggler.value);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        // BlendShapeModifier
        private static void ToClipDefault(this BlendShapeNameValue namevalue, AnimationClip clip, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var binding = CreateBlendShapeBinding(skinnedMeshRenderer, namevalue.name);
            var value = skinnedMeshRenderer.GetBlendShapeWeight(skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(namevalue.name));
            var curve = SimpleCurve(value);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        private static void ToClip(this BlendShapeNameValue namevalue, AnimationClip clip, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var binding = CreateBlendShapeBinding(skinnedMeshRenderer, namevalue.name);
            var curve = SimpleCurve(namevalue.value);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        // MaterialReplacer
        private static void ToClipDefault(this MaterialReplacer replacer, AnimationClip clip)
        {
            for(int i = 0; i < replacer.replaceTo.Length; i++)
            {
                if(!replacer.replaceTo[i]) continue;
                var binding = CreateMaterialReplaceBinding(replacer.renderer, i);
                var curve = SimpleCurve(replacer.renderer.sharedMaterials[i]);
                AnimationUtility.SetObjectReferenceCurve(clip, binding, curve);
            }
        }

        private static void ToClip(this MaterialReplacer replacer, AnimationClip clip)
        {
            for(int i = 0; i < replacer.replaceTo.Length; i++)
            {
                if(!replacer.replaceTo[i]) continue;
                var binding = CreateMaterialReplaceBinding(replacer.renderer, i);
                var curve = SimpleCurve(replacer.replaceTo[i]);
                AnimationUtility.SetObjectReferenceCurve(clip, binding, curve);
            }
        }

        // MaterialPropertyModifier
        private static void ToClipDefault(this MaterialPropertyModifier modifier, AnimationClip clip)
        {
            foreach(var renderer in modifier.renderers)
            {
                if(!renderer) continue;
                foreach(var floatModifier in modifier.floatModifiers)
                {
                    var binding = CreateMaterialPropertyBinding(renderer, floatModifier.propertyName);
                    float value = 0;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(!material.HasProperty(floatModifier.propertyName)) continue;
                        value = material.GetFloat(floatModifier.propertyName);
                        break;
                    }
                    var curve = SimpleCurve(value);
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                }
                foreach(var vectorModifier in modifier.vectorModifiers)
                {
                    var bindingX = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.x");
                    var bindingY = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.y");
                    var bindingZ = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.z");
                    var bindingW = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.w");
                    Vector4 value = Vector4.zero;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(!material.HasProperty(vectorModifier.propertyName)) continue;
                        value = material.GetVector(vectorModifier.propertyName);
                        break;
                    }
                    var curveX = SimpleCurve(value.x);
                    var curveY = SimpleCurve(value.y);
                    var curveZ = SimpleCurve(value.z);
                    var curveW = SimpleCurve(value.w);
                    AnimationUtility.SetEditorCurve(clip, bindingX, curveX);
                    AnimationUtility.SetEditorCurve(clip, bindingY, curveY);
                    AnimationUtility.SetEditorCurve(clip, bindingZ, curveZ);
                    AnimationUtility.SetEditorCurve(clip, bindingW, curveW);
                }
            }
        }

        private static void ToClip(this MaterialPropertyModifier modifier, AnimationClip clip)
        {
            foreach(var renderer in modifier.renderers)
            {
                if(!renderer) continue;
                foreach(var floatModifier in modifier.floatModifiers)
                {
                    var binding = CreateMaterialPropertyBinding(renderer, floatModifier.propertyName);
                    var curve = SimpleCurve(floatModifier.value);
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                }
                foreach(var vectorModifier in modifier.vectorModifiers)
                {
                    var bindingX = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.x");
                    var bindingY = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.y");
                    var bindingZ = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.z");
                    var bindingW = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.w");
                    var curveX = SimpleCurve(vectorModifier.value.x);
                    var curveY = SimpleCurve(vectorModifier.value.y);
                    var curveZ = SimpleCurve(vectorModifier.value.z);
                    var curveW = SimpleCurve(vectorModifier.value.w);
                    AnimationUtility.SetEditorCurve(clip, bindingX, curveX);
                    AnimationUtility.SetEditorCurve(clip, bindingY, curveY);
                    AnimationUtility.SetEditorCurve(clip, bindingZ, curveZ);
                    AnimationUtility.SetEditorCurve(clip, bindingW, curveW);
                }
            }
        }
    }
}
