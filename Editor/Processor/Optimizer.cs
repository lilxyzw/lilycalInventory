using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace jp.lilxyzw.lilycalinventory
{
    internal class Optimizer
    {
        internal static void OptimizeMaterials(Material[] materials)
        {
            var propMap = materials.Select(m => m.shader).Distinct().Where(s => s).ToDictionary(s => s, s => new ShaderPropertyContainer(s));
            foreach(var m in materials) RemoveUnusedProperties(m, propMap);
        }

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
            for(int i = props.arraySize - 1; i >= 0; i--)
                if(!names.Contains(props.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue))
                    props.DeleteArrayElementAtIndex(i);
        }

        private static void DeleteAll(SerializedProperty props)
        {
            for(int i = props.arraySize - 1; i >= 0; i--)
                props.DeleteArrayElementAtIndex(i);
        }
    }

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
