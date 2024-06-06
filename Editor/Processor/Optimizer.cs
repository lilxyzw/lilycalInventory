using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    internal class Optimizer
    {
        internal static void OptimizeMaterials(Material[] materials, BuildContext ctx)
        {
            var propMap = materials.Select(m => m.shader).Distinct().Where(s => s).ToDictionary(s => s, s => new ShaderPropertyContainer(s));

            #if LIL_TOON_1_8_0
            var clips = new HashSet<AnimationClip>();
            clips.UnionWith(ctx.AvatarRootObject.GetComponentsInChildren<Animator>(true).Where(a => a.runtimeAnimatorController).SelectMany(a => a.runtimeAnimatorController.animationClips));
            #if LIL_VRCSDK3A
            var descriptor = ctx.AvatarDescriptor;
            clips.UnionWith(descriptor.specialAnimationLayers.Where(l => l.animatorController).SelectMany(l => l.animatorController.animationClips));
            if(descriptor.customizeAnimationLayers) clips.UnionWith(descriptor.baseAnimationLayers.Where(l => l.animatorController).SelectMany(l => l.animatorController.animationClips));
            #endif
            var props = clips.SelectMany(c => AnimationUtility.GetCurveBindings(c)).Select(b => b.propertyName).Where(n => n.Contains("material."))
            .Select(n => n=n.Substring("material.".Length))
            .Select(n => {if(n.Contains(".")) n=n.Substring(0, n.IndexOf(".")); return n;}).Distinct().ToArray();
            #endif

            foreach(var m in materials)
            {
                RemoveUnusedProperties(m, propMap);
                #if LIL_TOON_1_8_0
                if(lilToon.lilMaterialUtils.CheckShaderIslilToon(m)) lilToon.lilMaterialUtils.RemoveUnusedTextureOnly(m, m.shader.name.Contains("Lite"), props);
                #endif
            }
        }

        // シェーダーで使われていないプロパティを除去
        private static void RemoveUnusedProperties(Material material, Dictionary<Shader, ShaderPropertyContainer> propMap)
        {
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            if(material.shader)
            {
                var dic = propMap[material.shader];
                DeleteUnused(savedProps.FindPropertyRelative("m_TexEnvs"), dic.textures);
                DeleteUnused(savedProps.FindPropertyRelative("m_Floats"), dic.floats);
                DeleteUnused(savedProps.FindPropertyRelative("m_Colors"), dic.vectors);
            }
            else
            {
                DeleteAll(savedProps.FindPropertyRelative("m_TexEnvs"));
                DeleteAll(savedProps.FindPropertyRelative("m_Floats"));
                DeleteAll(savedProps.FindPropertyRelative("m_Colors"));
            }
            so.ApplyModifiedProperties();
        }

        private static void DeleteUnused(SerializedProperty props, List<string> names)
        {
            if(props.arraySize == 0) return;
            var ints = new List<int>();
            props.DoAllElements((p,i) => {
                if(!names.Contains(p.FPR("first").stringValue)) ints.Add(i);
            });
            ints.Reverse();
            foreach(var a in ints) props.DeleteArrayElementAtIndex(a);
        }

        private static void DeleteAll(SerializedProperty props)
        {
            for(int i = props.arraySize - 1; i >= 0; i--)
                props.DeleteArrayElementAtIndex(i);
        }
    }

    // シェーダーのプロパティを検索して保持するクラス
    internal class ShaderPropertyContainer
    {
        internal List<string> textures = new List<string>();
        internal List<string> floats = new List<string>();
        internal List<string> vectors = new List<string>();

        internal ShaderPropertyContainer(Shader shader)
        {
            textures = new List<string>();
            floats = new List<string>();
            vectors = new List<string>();

            var count = shader.GetPropertyCount();
            for(int i = 0; i < count; i++)
            {
                var t = shader.GetPropertyType(i);
                var name = shader.GetPropertyName(i);
                if(t == ShaderPropertyType.Texture) textures.Add(name);
                else if(t == ShaderPropertyType.Color || t == ShaderPropertyType.Vector) vectors.Add(name);
                else floats.Add(name);
            }
        }
    }
}
