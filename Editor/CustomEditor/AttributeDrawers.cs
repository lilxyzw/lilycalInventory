using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    // プレースホルダーで親フォルダを表示
    [CustomPropertyDrawer(typeof(MenuFolderOverrideAttribute))]
    internal class MenuFolderOverrideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string parentName = "";
            var gameObject = (property.serializedObject.targetObject as Component).gameObject;
            if(property.serializedObject.targetObject is MenuBaseComponent c && !string.IsNullOrEmpty(c.menuName) && c.menuName.Contains("/"))
            {
                parentName = "/" + c.menuName.Substring(0, c.menuName.LastIndexOf("/"));
            }

            if(property.objectReferenceValue)
            {
                parentName = property.objectReferenceValue.name + parentName;
            }
            else if(property.serializedObject.targetObject is AutoDresser && string.IsNullOrEmpty(parentName))
            {
                var root = gameObject.GetAvatarRoot();
                if(root)
                {
                    var settings = root.GetComponentInChildren<AutoDresserSettings>();
                    if(settings) parentName = settings.GetMenuName();
                }
                if(string.IsNullOrEmpty(parentName)) parentName = "AutoDresser";
            }
            else
            {
                var parent = gameObject.GetComponentInParentInAvatar<MenuFolder>();
                if(parent) parentName = parent.GetMenuName() + parentName;
            }
            if(string.IsNullOrEmpty(parentName)) parentName = "(Root)";
            if(parentName.StartsWith("/")) parentName = parentName.Substring(1);
            GUIHelper.ObjectField(position, Localization.G(property), property, $"{parentName} (Menu Folder)");
        }
    }

    // Vector4を1行で表示
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

    // ラベルなし
    [CustomPropertyDrawer(typeof(NoLabelAttribute))]
    internal class NoLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }
    }

    // プレースホルダーでメニュー名を表示
    [CustomPropertyDrawer(typeof(MenuNameAttribute))]
    internal class MenuNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isGenerateParameter = property.serializedObject.targetObject is IGenerateParameter;
            string key = isGenerateParameter ? "inspector.menuParameterName" : "inspector.menuName";
            #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
            bool overrideMA = property.serializedObject.GetObjectInProperty("parentOverrideMA");
            if(overrideMA && isGenerateParameter) key = "inspector.parameterName";
            if(overrideMA && !isGenerateParameter) EditorGUI.BeginDisabledGroup(true);
            #endif
            GUIHelper.TextField(position, Localization.G(key), property, property.serializedObject.targetObject.name);
            #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
            if(overrideMA && !isGenerateParameter) EditorGUI.EndDisabledGroup();
            #endif
        }
    }

    // プレースホルダーで衣装名を表示
    [CustomPropertyDrawer(typeof(CostumeNameAttribute))]
    internal class CostumeNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var path = property.propertyPath.Substring(0, property.propertyPath.Length - property.name.Length);
            using var copy = property.serializedObject.FindProperty(path + "parametersPerMenu");
            string name = property.stringValue;

            if(string.IsNullOrEmpty(name))
            {
                var autoDresser = property.serializedObject.GetObjectInProperty(path + "autoDresser") as AutoDresser;
                if(autoDresser) name = autoDresser.GetMenuName();
            }

            if(string.IsNullOrEmpty(name))
            {
                using var togglers = copy.FPR("objects");
                for(int i = 0; i < togglers.arraySize; i++)
                {
                    var obj = togglers.GetObjectInProperty(i, "obj");
                    if(!obj || string.IsNullOrEmpty(obj.name)) continue;
                    name = obj.name;
                    break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                using var modifiers = copy.FPR("blendShapeModifiers");
                for(int i = 0; i < modifiers.arraySize; i++)
                {
                    using var nv = modifiers.GetPropertyInArrayElement(i, "blendShapeNameValues");
                    for(int j = 0; j < nv.arraySize; j++)
                    {
                        var nameTemp = nv.GetStringInProperty(j, "name");
                        if(string.IsNullOrEmpty(nameTemp)) continue;
                        name = nameTemp;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                using var replacers = copy.FPR("materialReplacers");
                for(int i = 0; i < replacers.arraySize; i++)
                {
                    using var t = replacers.GetPropertyInArrayElement(i, "replaceTo");
                    for(int j = 0; j < t.arraySize; j++)
                    {
                        var m = t.GetObjectInProperty(j);
                        if(!m || string.IsNullOrEmpty(m.name)) continue;
                        name = m.name;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                using var modifiers = copy.FPR("materialPropertyModifiers");
                for(int i = 0; i < modifiers.arraySize; i++)
                {
                    using var m = modifiers.GetPropertyInArrayElement(i, "floatModifiers");
                    for(int j = 0; j < m.arraySize; j++)
                    {
                        var n = m.GetStringInProperty(j, "propertyName");
                        if(string.IsNullOrEmpty(n)) continue;
                        name = n;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                using var modifiers = copy.FPR("materialPropertyModifiers");
                for(int i = 0; i < modifiers.arraySize; i++)
                {
                    using var m = modifiers.GetPropertyInArrayElement(i, "vectorModifiers");
                    for(int j = 0; j < m.arraySize; j++)
                    {
                        var n = m.GetStringInProperty(j, "propertyName");
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

    // フレーム値をパーセント表記で表示
    [CustomPropertyDrawer(typeof(FrameAttribute))]
    internal class FrameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.Slider(position, Localization.G(property), property.floatValue * 100f, 0f, 100f);
            if(EditorGUI.EndChangeCheck()) property.floatValue = value / 100f;
        }
    }

    // プロパティをboxで囲んで表示
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

    // パラメーターのデフォルト値を表示
    [CustomPropertyDrawer(typeof(DefaultValueAttribute))]
    internal class DefaultValueDrawer : PropertyDrawer
    {
        private static readonly GUIContent[] boolOptions = new[] { new GUIContent(false.ToString()), new GUIContent(true.ToString()) };
        private GUIContent boolNonZeroWarningContent => EditorGUIUtility.TrTextContentWithIcon(Localization.S($"inspector.defaultBoolNonZeroWarning"), MessageType.Warning);
        private GUIContent intNonZeroWarningContent => EditorGUIUtility.TrTextContentWithIcon(Localization.S($"inspector.defaultIntNonZeroWarning"), MessageType.Warning);
        private float boolNonZeroWarningContentHeight => EditorStyles.helpBox.CalcHeight(boolNonZeroWarningContent, EditorGUIUtility.currentViewWidth);
        private float intNonZeroWarningContentHeight => EditorStyles.helpBox.CalcHeight(intNonZeroWarningContent, EditorGUIUtility.currentViewWidth);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.SingleLine();
            switch(property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    property.boolValue = EditorGUI.Popup(position, Localization.G("inspector.defaultBool"), property.boolValue ? 1 : 0, boolOptions) == 1;
                    if(property.boolValue)
                    {
                        position.NewLine();
                        position.height = boolNonZeroWarningContentHeight;
                        GUI.Label(position, boolNonZeroWarningContent, EditorStyles.helpBox);
                    }
                    break;
                case SerializedPropertyType.Integer:
                    var attr = attribute as DefaultValueAttribute;
                    using(var array = property.serializedObject.FindProperty(attr.array))
                    property.intValue = EditorGUI.IntSlider(position, Localization.G("inspector.defaultInt"), property.intValue, 0, array.arraySize - 1);
                    if(property.intValue != 0)
                    {
                        position.NewLine();
                        position.height = intNonZeroWarningContentHeight;
                        GUI.Label(position, intNonZeroWarningContent, EditorStyles.helpBox);
                    }
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var position = new Rect();
            position.SingleLine();
            switch(property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    if(property.boolValue)
                    {
                        position.NewLine();
                        position.height = boolNonZeroWarningContentHeight;
                    }
                    break;
                case SerializedPropertyType.Integer:
                    if(property.intValue != 0)
                    {
                        position.NewLine();
                        position.height = intNonZeroWarningContentHeight;
                    }
                    break;
            }
            return position.yMax;
        }
    }

    [CustomPropertyDrawer(typeof(LILDisableWhenAttribute))]
    internal class LILDisableWhenDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as LILDisableWhenAttribute;
            using var prop = property.serializedObject.FindProperty(attr.propertyPath);
            if(prop != null)
            {
                switch(prop.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        GUI.enabled = prop.boolValue != attr.boolValue;
                        break;
                    case SerializedPropertyType.Integer:
                        GUI.enabled = prop.intValue != attr.intValue;
                        break;
                    case SerializedPropertyType.Float:
                        GUI.enabled = prop.floatValue != attr.floatValue;
                        break;
                }
            }
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
    }
}
