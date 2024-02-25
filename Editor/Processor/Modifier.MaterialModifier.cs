using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
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
                
                var ignoreMaterials = modifier.ignoreMaterials.Where(m => m && Cloner.materialMap.ContainsKey(m)).Select(m => Cloner.materialMap[m]);
                var materialsMod = materials.Except(ignoreMaterials).ToArray();
                if(materialsMod.Length == 0) continue;

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

            var textures = savedProps.FindPropertyRelative("m_TexEnvs");
            for(int i = 0; i < textures.arraySize; i++)
            {
                var item = textures.GetArrayElementAtIndex(i);
                var name = item.FindPropertyRelative("first").stringValue;
                if(modifier.properties.Contains(name))
                    textureOverride[name] = item.FindPropertyRelative("second.m_Texture").objectReferenceValue;
            }

            var floats = savedProps.FindPropertyRelative("m_Floats");
            for(int i = 0; i < floats.arraySize; i++)
            {
                var item = floats.GetArrayElementAtIndex(i);
                var name = item.FindPropertyRelative("first").stringValue;
                if(modifier.properties.Contains(name))
                    floatOverride[name] = item.FindPropertyRelative("second").floatValue;
            }

            var vectors = savedProps.FindPropertyRelative("m_Colors");
            for(int i = 0; i < vectors.arraySize; i++)
            {
                var item = vectors.GetArrayElementAtIndex(i);
                var name = item.FindPropertyRelative("first").stringValue;
                if(modifier.properties.Contains(name))
                    vectorOverride[name] = item.FindPropertyRelative("second").colorValue;
            }
        }

        private static void ModifyProperties(Material material, Dictionary<string,Object> textureOverride, Dictionary<string,float> floatOverride, Dictionary<string,Vector4> vectorOverride)
        {
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            var textures = savedProps.FindPropertyRelative("m_TexEnvs");
            for(int i = 0; i < textures.arraySize; i++)
            {
                var item = textures.GetArrayElementAtIndex(i);
                var name = item.FindPropertyRelative("first").stringValue;
                if(textureOverride.ContainsKey(name))
                    item.FindPropertyRelative("second.m_Texture").objectReferenceValue = textureOverride[name];
            }

            var floats = savedProps.FindPropertyRelative("m_Floats");
            for(int i = 0; i < floats.arraySize; i++)
            {
                var item = floats.GetArrayElementAtIndex(i);
                var name = item.FindPropertyRelative("first").stringValue;
                if(floatOverride.ContainsKey(name))
                    item.FindPropertyRelative("second").floatValue = floatOverride[name];
            }

            var vectors = savedProps.FindPropertyRelative("m_Colors");
            for(int i = 0; i < vectors.arraySize; i++)
            {
                var item = vectors.GetArrayElementAtIndex(i);
                var name = item.FindPropertyRelative("first").stringValue;
                if(vectorOverride.ContainsKey(name))
                    item.FindPropertyRelative("second").colorValue = vectorOverride[name];
            }

            so.ApplyModifiedProperties();
        }
    }
}
