using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
                using var so = new SerializedObject(modifier.referenceMaterial);
                using var savedProps = so.FindProperty("m_SavedProperties");

                savedProps.FindPropertyRelative("m_TexEnvs").DoAllElements((p,i) => {
                    var name = p.GetStringInProperty("first");
                    if(modifier.properties.Contains(name))
                        textureOverride[name] = p.FindPropertyRelative("second.m_Texture").objectReferenceValue;
                });

                savedProps.FindPropertyRelative("m_Floats").DoAllElements((p,i) => {
                    var name = p.GetStringInProperty("first");
                    if(modifier.properties.Contains(name))
                        floatOverride[name] = p.FindPropertyRelative("second").floatValue;
                });

                savedProps.FindPropertyRelative("m_Colors").DoAllElements((p,i) => {
                    var name = p.GetStringInProperty("first");
                    if(modifier.properties.Contains(name))
                        vectorOverride[name] = p.FindPropertyRelative("second").colorValue;
                });
            }

            private static void ModifyProperties(Material material, Dictionary<string,Object> textureOverride, Dictionary<string,float> floatOverride, Dictionary<string,Vector4> vectorOverride)
            {
                using var so = new SerializedObject(material);
                so.Update();
                var savedProps = so.FindProperty("m_SavedProperties");

                savedProps.FindPropertyRelative("m_TexEnvs").DoAllElements((p,i) => {
                    var name = p.GetStringInProperty("first");
                    if(textureOverride.ContainsKey(name))
                        p.FindPropertyRelative("second.m_Texture").objectReferenceValue = textureOverride[name];
                });

                savedProps.FindPropertyRelative("m_Floats").DoAllElements((p,i) => {
                    var name = p.GetStringInProperty("first");
                    if(floatOverride.ContainsKey(name))
                        p.FindPropertyRelative("second").floatValue = floatOverride[name];
                });

                savedProps.FindPropertyRelative("m_Colors").DoAllElements((p,i) => {
                    var name = p.GetStringInProperty("first");
                    if(vectorOverride.ContainsKey(name))
                        p.FindPropertyRelative("second").colorValue = vectorOverride[name];
                });

                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
