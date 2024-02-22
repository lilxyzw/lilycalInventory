using System.Collections.Generic;
using System.Linq;
using jp.lilxyzw.materialmodifier.runtime;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.materialmodifier
{
    [CustomPropertyDrawer(typeof(LILElement), true)]
    internal class LILElementDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isExpanded = GUIHelper.FoldoutOnly(position, property);
            var end = property.GetEndProperty();
            property.NextVisible(true);
            bool isObjectArray = property.isArray && property.arrayElementType.StartsWith("PPtr");

            if(!isExpanded)
            {
                if(isObjectArray) EditorGUI.LabelField(position, string.Join(", ", property.GetAllObjectNames()));
                else EditorGUI.PropertyField(position.SetHeight(property), property);
                return;
            }

            if(isObjectArray) position.Indent();
            position = GUIHelper.AutoField(position, property);
            if(!isObjectArray) position.Indent();

            while(property.NextVisible(false) && !SerializedProperty.EqualContents(property, end))
            {
                position = GUIHelper.AutoField(position, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(!property.isExpanded) return GUIHelper.propertyHeight;
            var end = property.GetEndProperty();
            var iterator = property.Copy();
            iterator.NextVisible(true);
            float height = GUIHelper.GetAutoFieldHeight(iterator);
            while(iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += GUIHelper.GetAutoFieldHeight(iterator);
            }
            return height;
        }
    }

    // Without foldout
    [CustomPropertyDrawer(typeof(LILElementSimple), true)]
    internal class LILElementSimpleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var end = property.GetEndProperty();
            property.NextVisible(true);
            position = GUIHelper.AutoField(position, property);

            while(property.NextVisible(false) && !SerializedProperty.EqualContents(property, end))
            {
                position = GUIHelper.AutoField(position, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var end = property.GetEndProperty();
            var iterator = property.Copy();
            iterator.NextVisible(true);
            float height = GUIHelper.GetAutoFieldHeight(iterator);
            while(iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += GUIHelper.GetAutoFieldHeight(iterator);
            }
            return height;
        }
    }

    // This need to set initialize value
    [CustomPropertyDrawer(typeof(ParametersPerMenu))]
    internal class ParametersPerMenuDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = GUIHelper.List(position, property.FPR(ParametersPerMenu.N_objects), prop =>
                {
                    prop.FindPropertyRelative("obj").objectReferenceValue = null;
                    prop.FindPropertyRelative("value").boolValue = true;
                }
            );
            position = GUIHelper.List(position, property.FPR(ParametersPerMenu.N_blendShapeModifiers), prop =>
                {
                    prop.FindPropertyRelative("skinnedMeshRenderer").objectReferenceValue = null;
                    prop.FindPropertyRelative("blendShapeNameValues").arraySize = 0;
                }
            );
            position = GUIHelper.List(position, property.FPR(ParametersPerMenu.N_materialReplacers), prop =>
                {
                    prop.FindPropertyRelative("renderer").objectReferenceValue = null;
                    prop.FindPropertyRelative("replaceTo").arraySize = 0;
                }
            );
            position = GUIHelper.List(position, property.FPR(ParametersPerMenu.N_materialPropertyModifiers));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GUIHelper.GetListHeight(property.FPR("objects")) +
                GUIHelper.GetListHeight(property.FPR("blendShapeModifiers")) +
                GUIHelper.GetListHeight(property.FPR("materialReplacers")) +
                GUIHelper.GetListHeight(property.FPR("materialPropertyModifiers")) +
                GUIHelper.GetSpaceHeight(3);
        }
    }

    // This need to draw property on 1-line
    [CustomPropertyDrawer(typeof(ObjectToggler))]
    internal class ObjectTogglerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueRect = new Rect(position.x, position.y, 16, position.height);
            var objRect = new Rect(valueRect.xMax + 4, position.y, position.width - 20, position.height);
            GUIHelper.ChildFieldOnly(valueRect, property, "value");
            GUIHelper.ChildFieldOnly(objRect, property, "obj");
        }
    }

    [CustomPropertyDrawer(typeof(BlendShapeModifier))]
    internal class BlendShapeModifierDrawer : PropertyDrawer
    {
        readonly Dictionary<Mesh, string[]> blendShapes = new Dictionary<Mesh, string[]>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var smr = property.FPR("skinnedMeshRenderer");
            EditorGUI.PropertyField(position.SingleLine(), smr);
            Mesh mesh = null;
            if(smr.objectReferenceValue) mesh = ((SkinnedMeshRenderer)smr.objectReferenceValue).sharedMesh;
            BlendShapeNameValueDrawer.mesh = mesh;

            if(!GUIHelper.FoldoutOnly(position, property)) return;

            position.Indent();
            var blendShapeNameValues = property.FPR("blendShapeNameValues");
            position = GUIHelper.List(position.NewLine(), blendShapeNameValues, prop =>
                {
                    if(!mesh) return;
                    blendShapeNameValues.arraySize--;
                    UpdateBlendShapes(mesh);
                    EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.one), GUIHelper.CreateContents(blendShapes[mesh]), -1, (userData, options, selected) =>
                    {
                        blendShapeNameValues.arraySize++;
                        var p = blendShapeNameValues.GetArrayElementAtIndex(blendShapeNameValues.arraySize - 1);
                        p.FPR("name").stringValue = blendShapes[mesh][selected];
                        blendShapeNameValues.serializedObject.ApplyModifiedProperties();
                    }, null);
                }
            );
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(!property.isExpanded) return GUIHelper.propertyHeight;
            return GUIHelper.propertyHeight +
                GUIHelper.GetListHeight(property.FPR("blendShapeNameValues")) +
                GUIHelper.GetSpaceHeight(2);
        }

        private void UpdateBlendShapes(Mesh mesh)
        {
            if(blendShapes.ContainsKey(mesh) && blendShapes[mesh].Length == mesh.blendShapeCount) return;
    
            var blendShapeList = new List<string>();
            for(int i = 0; i < mesh.blendShapeCount; i++)
                blendShapeList.Add(mesh.GetBlendShapeName(i));
            blendShapes[mesh] = blendShapeList.ToArray();
        }
    }

    // This need to draw property on 1-line
    [CustomPropertyDrawer(typeof(BlendShapeNameValue))]
    internal class BlendShapeNameValueDrawer : PropertyDrawer
    {
        public static Mesh mesh = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var colors = EditorStyles.textField.GetColors();
            if(mesh && mesh.GetBlendShapeIndex(property.FPR("name").stringValue) == -1) EditorStyles.textField.SetColors(Color.red);
            var nameRect = new Rect(position.x, position.y, position.width * 0.666f, position.height);
            var valueRect = new Rect(nameRect.xMax + 2, position.y, position.width * 0.333f - 2, position.height);
            EditorGUIUtility.labelWidth = 40;
            GUIHelper.ChildField(nameRect, property, "name");
            GUIHelper.ChildField(valueRect, property, "value");
            EditorGUIUtility.labelWidth = 0;
            EditorStyles.textField.SetColors(colors);
        }
    }

    [CustomPropertyDrawer(typeof(MaterialReplacer))]
    internal class MaterialReplacerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var renderer = property.FPR("renderer");
            var replaceTo = property.FPR("replaceTo");
            GUIHelper.FieldOnly(position.SingleLine(), renderer);

            var materials = new Material[0];
            if(renderer.objectReferenceValue)
            {
                materials = ((Renderer)renderer.objectReferenceValue).sharedMaterials;
            }
            replaceTo.ResizeArray(materials.Length, p => p.objectReferenceValue = null);

            if(!GUIHelper.FoldoutOnly(position, property)) return;

            EditorGUI.LabelField(position.NewLine(), Localization.G("inspector.replaceTo"));
            position.Indent();
            position = GUIHelper.SimpleList(replaceTo, position.NewLine(), materials.Select(m => m.TryGetName()).ToArray());
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(!property.isExpanded) return GUIHelper.propertyHeight;
            return GUIHelper.propertyHeight * (property.FPR("replaceTo").arraySize + 2) + GUIHelper.GetSpaceHeight(3);
        }
    }
}
