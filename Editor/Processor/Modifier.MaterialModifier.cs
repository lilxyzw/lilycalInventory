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

    internal partial class Modifier
    {
        internal static void ApplyMaterialModifier(Material[] materials, MaterialModifier[] modifiers)
        {
            foreach(var modifier in modifiers)
            {
                var refMaterial = modifier.referenceMaterial;
                if(!refMaterial || !refMaterial.shader) continue;

                #if LIL_NDMF
                var materialsMod = materials.Where(m => !modifier.ignoreMaterials.Contains(ObjectRegistry.GetReference(m).Object as Material)).ToArray();
                if(materialsMod.Length == 0) continue;
                #else
                var ignoreMaterials = modifier.ignoreMaterials.Where(m => m && Cloner.materialMap.ContainsKey(m)).Select(m => Cloner.materialMap[m]);
                var materialsMod = materials.Except(ignoreMaterials).ToArray();
                #endif

                var textureOverride = new Dictionary<string,Object>();
                var floatOverride = new Dictionary<string,float>();
                var vectorOverride = new Dictionary<string,Vector4>();
                GatherProperties(modifier, textureOverride, floatOverride, vectorOverride);

                foreach(var material in materialsMod)
                {
                    ModifyProperties(material, textureOverride, floatOverride, vectorOverride);
                }
            }
        }

        private static void GatherProperties(MaterialModifier modifier, Dictionary<string,Object> textureOverride, Dictionary<string,float> floatOverride, Dictionary<string,Vector4> vectorOverride)
        {
            var so = new SerializedObject(modifier.referenceMaterial);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            savedProps.FindPropertyRelative("m_TexEnvs").DoAllElements((p,i) => {
                var name = p.FPR("first").stringValue;
                if(modifier.properties.Contains(name))
                    textureOverride[name] = p.FindPropertyRelative("second.m_Texture").objectReferenceValue;
            });

            savedProps.FindPropertyRelative("m_Floats").DoAllElements((p,i) => {
                var name = p.FPR("first").stringValue;
                if(modifier.properties.Contains(name))
                    floatOverride[name] = p.FindPropertyRelative("second").floatValue;
            });

            savedProps.FindPropertyRelative("m_Colors").DoAllElements((p,i) => {
                var name = p.FPR("first").stringValue;
                if(modifier.properties.Contains(name))
                    vectorOverride[name] = p.FindPropertyRelative("second").colorValue;
            });
        }

        private static void ModifyProperties(Material material, Dictionary<string,Object> textureOverride, Dictionary<string,float> floatOverride, Dictionary<string,Vector4> vectorOverride)
        {
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            savedProps.FindPropertyRelative("m_TexEnvs").DoAllElements((p,i) => {
                var name = p.FPR("first").stringValue;
                if(textureOverride.ContainsKey(name))
                    p.FindPropertyRelative("second.m_Texture").objectReferenceValue = textureOverride[name];
            });

            savedProps.FindPropertyRelative("m_Floats").DoAllElements((p,i) => {
                var name = p.FPR("first").stringValue;
                if(floatOverride.ContainsKey(name))
                    p.FindPropertyRelative("second").floatValue = floatOverride[name];
            });

            savedProps.FindPropertyRelative("m_Colors").DoAllElements((p,i) => {
                var name = p.FPR("first").stringValue;
                if(vectorOverride.ContainsKey(name))
                    p.FindPropertyRelative("second").colorValue = vectorOverride[name];
            });

            so.ApplyModifiedProperties();
        }
    }
}
