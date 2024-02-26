using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    [CustomPropertyDrawer(typeof(MenuFolderOverrideAttribute))]
    internal class MenuFolderOverrideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string parentName;
            var gameObject = (property.serializedObject.targetObject as Component).gameObject;
            if(property.objectReferenceValue)
            {
                parentName = property.objectReferenceValue.name;
            }
            else
            {
                var parent = gameObject.GetComponentInParentInAvatar<MenuFolder>();
                if(parent) parentName = parent.gameObject.name;
                else parentName = "(Root)";
            }
            EditorGUI.LabelField(position.SingleLine(), Localization.G("inspector.parentFolder"), new GUIContent(parentName));
            position.Indent();
            EditorGUI.PropertyField(position.NewLine(), property, Localization.G(property));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GUIHelper.propertyHeight * 2 + GUIHelper.GetSpaceHeight(2);
        }
    }

    [CustomPropertyDrawer(typeof(OneLineVectorAttribute))]
    internal class OneLineVectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            bool wideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            var vec = EditorGUI.Vector4Field(position, Localization.G(property), property.vector4Value);
            EditorGUIUtility.wideMode = wideMode;
            if(EditorGUI.EndChangeCheck())
            {
                property.vector4Value = vec;
            }
        }
    }

    [CustomPropertyDrawer(typeof(NoLabelAttribute))]
    internal class NoLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(MenuNameAttribute))]
    internal class MenuNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIHelper.TextField(position, Localization.G(property), property, property.serializedObject.targetObject.name);
        }
    }

    [CustomPropertyDrawer(typeof(CostumeNameAttribute))]
    internal class CostumeNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var copy = property.Copy();
            copy.Next(false);
            copy.Next(false);
            string name = property.stringValue;

            if(string.IsNullOrEmpty(name))
            {
                var togglers = copy.FPR("objects");
                for(int i = 0; i < togglers.arraySize; i++)
                {
                    var obj = togglers.GetArrayElementAtIndex(i).FPR("obj").objectReferenceValue;
                    if(!obj || string.IsNullOrEmpty(obj.name)) continue;
                    name = obj.name;
                    break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var modifiers = copy.FPR("blendShapeModifiers");
                for(int i = 0; i < modifiers.arraySize; i++)
                {
                    var nv = modifiers.GetArrayElementAtIndex(i).FPR("blendShapeNameValues");
                    for(int j = 0; j < nv.arraySize; j++)
                    {
                        var nameTemp = nv.GetArrayElementAtIndex(j).FPR("name").stringValue;
                        if(string.IsNullOrEmpty(nameTemp)) continue;
                        name = nameTemp;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var replacers = copy.FPR("materialReplacers");
                for(int i = 0; i < replacers.arraySize; i++)
                {
                    var t = replacers.GetArrayElementAtIndex(i).FPR("replaceTo");
                    for(int j = 0; j < t.arraySize; j++)
                    {
                        var m = t.GetArrayElementAtIndex(j).objectReferenceValue;
                        if(!m || string.IsNullOrEmpty(m.name)) continue;
                        name = m.name;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var replacers = copy.FPR("materialPropertyModifiers");
                for(int i = 0; i < replacers.arraySize; i++)
                {
                    var m = replacers.GetArrayElementAtIndex(i).FPR("floatModifiers");
                    for(int j = 0; j < m.arraySize; j++)
                    {
                        var n = m.GetArrayElementAtIndex(j).FPR("propertyName").stringValue;
                        if(string.IsNullOrEmpty(n)) continue;
                        name = n;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var replacers = copy.FPR("materialPropertyModifiers");
                for(int i = 0; i < replacers.arraySize; i++)
                {
                    var m = replacers.GetArrayElementAtIndex(i).FPR("vectorModifiers");
                    for(int j = 0; j < m.arraySize; j++)
                    {
                        var n = m.GetArrayElementAtIndex(j).FPR("propertyName").stringValue;
                        if(string.IsNullOrEmpty(n)) continue;
                        name = n;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                name = Localization.S("inspector.menuNameEmpty");
            }

            if(!string.IsNullOrEmpty(property.stringValue)) name = " ";

            GUIHelper.TextField(position, Localization.G(property), property, name);
        }
    }

    [CustomPropertyDrawer(typeof(LILBoxAttribute))]
    internal class LILBoxDrawer : PropertyDrawer
    {
        ParametersPerMenuDrawer parametersPerMenuDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.BeginGroup(position, EditorStyles.helpBox);
            position = EditorStyles.helpBox.padding.Remove(position);
            position = EditorStyles.helpBox.padding.Remove(position);
            position.Indent(1);
            GUI.EndGroup();
            if(parametersPerMenuDrawer == null) parametersPerMenuDrawer = new ParametersPerMenuDrawer();
            parametersPerMenuDrawer.OnGUI(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(parametersPerMenuDrawer == null) parametersPerMenuDrawer = new ParametersPerMenuDrawer();
            return parametersPerMenuDrawer.GetPropertyHeight(property, label) + EditorStyles.helpBox.padding.vertical * 2;
        }
    }
}
