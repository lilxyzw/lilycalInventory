using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class AnimationHelper
    {
        internal static (InternalClip,InternalClip) CreateClip(this ParametersPerMenu parameter, GameObject gameObject, string name)
        {
            var clipOff = new InternalClip();
            var clipOn = new InternalClip();
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
                    var renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
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
                if(modifier.renderers.Length == 0)
                    modifier.renderers = gameObject.GetComponentsInChildren<Renderer>(true).ToArray();

                modifier.ToClipDefault(clipOff);
                modifier.ToClip(clipOn, clipOff);
            }
            return (clipOff, clipOn);
        }

        internal static (InternalClip,InternalClip) CreateClip(this ParametersPerMenu parameter, BuildContext ctx, string name)
        {
            return parameter.CreateClip(ctx.AvatarRootObject, name);
        }

        // ObjectToggler
        internal static void ToClipDefault(this ObjectToggler toggler, InternalClip clip)
        {
            var binding = CreateToggleBinding(toggler.obj);
            clip.Add(binding, !toggler.value);
            toggler.obj.SetActive(!toggler.value);
        }

        internal static void ToClip(this ObjectToggler toggler, InternalClip clip)
        {
            var binding = CreateToggleBinding(toggler.obj);
            clip.Add(binding, toggler.value);
        }

        // BlendShapeModifier
        private static void ToClipDefault(this BlendShapeNameValue namevalue, InternalClip clip, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var binding = CreateBlendShapeBinding(skinnedMeshRenderer, namevalue.name);
            var value = skinnedMeshRenderer.GetBlendShapeWeight(skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(namevalue.name));
            clip.Add(binding, value);
        }

        private static void ToClip(this BlendShapeNameValue namevalue, InternalClip clip, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var binding = CreateBlendShapeBinding(skinnedMeshRenderer, namevalue.name);
            clip.Add(binding, namevalue.value);
        }

        // MaterialReplacer
        private static void ToClipDefault(this MaterialReplacer replacer, InternalClip clip)
        {
            for(int i = 0; i < replacer.replaceTo.Length; i++)
            {
                if(!replacer.replaceTo[i]) continue;
                var binding = CreateMaterialReplaceBinding(replacer.renderer, i);
                clip.Add(binding, replacer.renderer.sharedMaterials[i]);
            }
        }

        private static void ToClip(this MaterialReplacer replacer, InternalClip clip)
        {
            for(int i = 0; i < replacer.replaceTo.Length; i++)
            {
                if(!replacer.replaceTo[i]) continue;
                var binding = CreateMaterialReplaceBinding(replacer.renderer, i);
                clip.Add(binding, replacer.replaceTo[i]);
            }
        }

        // MaterialPropertyModifier
        private static void ToClipDefault(this MaterialPropertyModifier modifier, InternalClip clip)
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
                        if(!material || !material.HasProperty(floatModifier.propertyName)) continue;
                        value = material.GetFloat(floatModifier.propertyName);
                        break;
                    }
                    clip.Add(binding, value);
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
                        if(!material || !material.HasProperty(vectorModifier.propertyName)) continue;
                        value = material.GetVector(vectorModifier.propertyName);
                        break;
                    }
                    clip.Add(bindingX, value.x);
                    clip.Add(bindingY, value.y);
                    clip.Add(bindingZ, value.z);
                    clip.Add(bindingW, value.w);
                }
            }
        }

        private static void ToClip(this MaterialPropertyModifier modifier, InternalClip clip, InternalClip clipDefault)
        {
            foreach(var renderer in modifier.renderers)
            {
                if(!renderer) continue;
                foreach(var floatModifier in modifier.floatModifiers)
                {
                    var binding = CreateMaterialPropertyBinding(renderer, floatModifier.propertyName);
                    clip.Add(binding, floatModifier.value);
                }
                foreach(var vectorModifier in modifier.vectorModifiers)
                {
                    var bindingX = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.x");
                    var bindingY = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.y");
                    var bindingZ = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.z");
                    var bindingW = CreateMaterialPropertyBinding(renderer, $"{vectorModifier.propertyName}.w");
                    clip.Add(bindingX, !vectorModifier.disableX ? vectorModifier.value.x : clipDefault.bindings[bindingX].Item1);
                    clip.Add(bindingY, !vectorModifier.disableY ? vectorModifier.value.y : clipDefault.bindings[bindingY].Item1);
                    clip.Add(bindingZ, !vectorModifier.disableZ ? vectorModifier.value.z : clipDefault.bindings[bindingZ].Item1);
                    clip.Add(bindingW, !vectorModifier.disableW ? vectorModifier.value.w : clipDefault.bindings[bindingW].Item1);
                }
            }
        }

        internal static ParametersPerMenu CreateDefaultParameters(this ParametersPerMenu[] parameters)
        {
            var parameter = new ParametersPerMenu();
            parameter.objects = parameters.SelectMany(p => p.objects).Select(o => o.obj).Distinct().Select(o => new ObjectToggler{obj = o, value = false}).ToArray();

            var blendShapeModifiers = parameters.SelectMany(p => p.blendShapeModifiers).Where(b => b.skinnedMeshRenderer && b.skinnedMeshRenderer.sharedMesh).Select(b => new BlendShapeModifier{skinnedMeshRenderer = b.skinnedMeshRenderer, blendShapeNameValues = b.blendShapeNameValues});
            foreach(var b in blendShapeModifiers)
            {
                b.blendShapeNameValues.Select(v => {
                    var index = b.skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(v.name);
                    if(index != -1) v.value = b.skinnedMeshRenderer.GetBlendShapeWeight(index);
                    return v;
                });
            }
            parameter.blendShapeModifiers = blendShapeModifiers.ToArray();

            parameter.materialReplacers = parameters.SelectMany(p => p.materialReplacers).Where(m => m.renderer).Select(m => new MaterialReplacer{renderer = m.renderer, replaceTo = m.renderer.sharedMaterials}).ToArray();
            var materialPropertyModifiers = parameters.SelectMany(p => p.materialPropertyModifiers);
            foreach(var modifier in materialPropertyModifiers)
            foreach(var renderer in modifier.renderers)
            {
                if(!renderer) continue;
                foreach(var floatModifier in modifier.floatModifiers)
                {
                    float value = 0;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(!material || !material.HasProperty(floatModifier.propertyName)) continue;
                        value = material.GetFloat(floatModifier.propertyName);
                        break;
                    }
                    floatModifier.value = value;
                }
                foreach(var vectorModifier in modifier.vectorModifiers)
                {
                    Vector4 value = Vector4.zero;
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if(!material || !material.HasProperty(vectorModifier.propertyName)) continue;
                        value = material.GetVector(vectorModifier.propertyName);
                        break;
                    }
                    vectorModifier.value = value;
                }
            }
            parameter.materialPropertyModifiers = materialPropertyModifiers.ToArray();

            return parameter;
        }

        internal static ParametersPerMenu Merge(this ParametersPerMenu parameter1, ParametersPerMenu parameter2)
        {
            var parameter = new ParametersPerMenu();
            var objs = parameter1.objects.Select(o => o.obj);
            parameter.objects = parameter1.objects.Union(parameter2.objects.Where(t => !objs.Contains(t.obj))).ToArray();
            var smrs = parameter1.blendShapeModifiers.Select(m => m.skinnedMeshRenderer);
            parameter.blendShapeModifiers = parameter1.blendShapeModifiers.Union(parameter2.blendShapeModifiers.Where(m => !smrs.Contains(m.skinnedMeshRenderer))).ToArray();
            var rs = parameter1.materialReplacers.Select(m => m.renderer);
            parameter.materialReplacers = parameter1.materialReplacers.Union(parameter2.materialReplacers.Where(m => !rs.Contains(m.renderer))).ToArray();
            parameter.materialPropertyModifiers = (MaterialPropertyModifier[])parameter1.materialPropertyModifiers.Clone();
            return parameter;
        }

        // TODO: Support other than toggler
        internal static void GatherConditions(this ItemToggler[] itemTogglers, Dictionary<GameObject, HashSet<string>> dic)
        {
            foreach(var itemToggler in itemTogglers)
                foreach(var toggler in itemToggler.parameter.objects)
                    dic.GetOrAdd(toggler.obj).Add(itemToggler.menuName);
        }

        internal static void GatherConditions(this CostumeChanger[] costumeChangers, Dictionary<GameObject, Dictionary<string, (int,HashSet<(int,bool)>)>> dic)
        {
            foreach(var costumeChanger in costumeChangers)
            {
                for(int i = 0; i < costumeChanger.costumes.Length; i++)
                {
                    var parameter = costumeChanger.costumes[i].parametersPerMenu;
                    foreach(var toggler in parameter.objects)
                    {
                        dic.GetOrAdd(toggler.obj).GetOrAdd(costumeChanger.menuName, costumeChanger.costumes.Length).Item2.Add((i, toggler.value));
                    }
                }
            }
        }

        private static Dictionary<TValue,TValue2> GetOrAdd<TKey,TValue,TValue2>(this Dictionary<TKey,Dictionary<TValue,TValue2>> dic, TKey key)
        {
            if(!dic.ContainsKey(key)) dic[key] = new Dictionary<TValue,TValue2>();
            return dic[key];
        }

        private static HashSet<TValue> GetOrAdd<TKey,TValue>(this Dictionary<TKey,HashSet<TValue>> dic, TKey key)
        {
            if(!dic.ContainsKey(key)) dic[key] = new HashSet<TValue>();
            return dic[key];
        }

        private static (int,HashSet<TValue>) GetOrAdd<TKey,TValue>(this Dictionary<TKey,(int,HashSet<TValue>)> dic, TKey key, int value)
        {
            if(!dic.ContainsKey(key)) dic[key] = (value,new HashSet<TValue>());
            return dic[key];
        }
    }
}
