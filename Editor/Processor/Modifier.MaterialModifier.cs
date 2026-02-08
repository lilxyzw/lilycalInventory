using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        internal static Dictionary<MaterialModifier, (Dictionary<string,Object> textureOverride, Dictionary<string,float> floatOverride, Dictionary<string,Vector4> vectorOverride)> modifierValues = new();

        private partial class Modifier
        {
            internal static void GetModifierValues(MaterialModifier[] modifiers)
            {
                modifierValues.Clear();
                foreach(var modifier in modifiers)
                {
                    // 参照マテリアルがないか壊れている場合は無視
                    var refMaterial = modifier.referenceMaterial;
                    if(!refMaterial || !refMaterial.shader) continue;

                    // 参照マテリアルからプロパティを取得
                    var textureOverride = new Dictionary<string,Object>();
                    var floatOverride = new Dictionary<string,float>();
                    var vectorOverride = new Dictionary<string,Vector4>();
                    GatherProperties(modifier, textureOverride, floatOverride, vectorOverride);

                    modifierValues[modifier] = (textureOverride, floatOverride, vectorOverride);
                }
            }

            internal static void ApplyMaterialModifier(Material[] materials)
            {
                foreach(var kv in modifierValues)
                {
                    var materialsMod = materials.Where(m => !kv.Key.ignoreMaterials.Contains(m) && !kv.Key.ignoreMaterials.Any(i => OriginEquals(i,m))).ToArray();
                    if(materialsMod.Length == 0) continue;

                    // 編集対象にプロパティをコピー
                    foreach(var material in materialsMod)
                    {
                        ModifyProperties(material, kv.Value.textureOverride, kv.Value.floatOverride, kv.Value.vectorOverride);
                    }
                }
            }

            internal static void GatherProperties(MaterialModifier modifier, Dictionary<string,Object> textureOverride, Dictionary<string,float> floatOverride, Dictionary<string,Vector4> vectorOverride)
            {
                var refMaterial = modifier.referenceMaterial;
                var shader = refMaterial.shader;
                var propertyCount = shader.GetPropertyCount();

                for(int i = 0; i < propertyCount; i++)
                {
                    var name = shader.GetPropertyName(i);
                    if(!modifier.properties.Contains(name)) continue;

                    var type = shader.GetPropertyType(i);
                    switch(type)
                    {
                        case ShaderPropertyType.Range:
                            // Fall through to Float case
                        case ShaderPropertyType.Float:
                            floatOverride[name] = refMaterial.GetFloat(name);
                            break;
                        case ShaderPropertyType.Texture:
                            textureOverride[name] = refMaterial.GetTexture(name);
                            break;
                        case ShaderPropertyType.Color:
                            vectorOverride[name] = refMaterial.GetColor(name);
                            break;
                        case ShaderPropertyType.Vector:
                            vectorOverride[name] = refMaterial.GetVector(name);
                            break;
                    }
                }
            }

            private static void ModifyProperties(Material material, Dictionary<string,Object> textureOverride, Dictionary<string,float> floatOverride, Dictionary<string,Vector4> vectorOverride)
            {
                foreach(var kv in textureOverride)
                {
                    if(material.HasProperty(kv.Key))
                        material.SetTexture(kv.Key, kv.Value as Texture);
                }

                foreach(var kv in floatOverride)
                {
                    if(material.HasProperty(kv.Key))
                        material.SetFloat(kv.Key, kv.Value);
                }

                foreach(var kv in vectorOverride)
                {
                    if(material.HasProperty(kv.Key))
                        material.SetVector(kv.Key, kv.Value);
                }
            }
        }
    }
}
