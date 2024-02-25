using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Globalization;
#if !UNITY_2020_1_OR_NEWER
using AnimationModeDriver = UnityEngine.Object;
#endif

namespace jp.lilxyzw.avatarmodifier
{
    using runtime;

    internal class PreviewHelper : ScriptableSingleton<PreviewHelper>
    {
        private static Component target;
        private static ParametersPerMenu m_Parameters;
        private static bool doPreview = true;
        private static int previewIndex = 0;

        [SerializeField] private AnimationModeDriver driver;
        private AnimationModeDriver Driver => driver ? driver : driver = CreateDriver();
        #if !UNITY_2020_1_OR_NEWER
        private AnimationModeDriver CreateDriver() => CreateInstance(typeof(AnimationMode).Assembly.GetType("UnityEditor.AnimationModeDriver"));
        #else
        private AnimationModeDriver CreateDriver() => CreateInstance<AnimationModeDriver>();
        #endif

        #if UNITY_2020_1_OR_NEWER
            private static void StartAnimationMode(AnimationModeDriver driver) => AnimationMode.StartAnimationMode(driver);
            private static void StopAnimationMode(AnimationModeDriver driver) => AnimationMode.StopAnimationMode(driver);
        #else
            private static MethodInfo startAnimationMode = null;
            private static MethodInfo stopAnimationMode = null;
            private static void StartAnimationMode(AnimationModeDriver driver)
            {
                if(startAnimationMode == null)
                    startAnimationMode = typeof(AnimationMode).GetMethod("StartAnimationMode", BindingFlags.Static | BindingFlags.NonPublic, null, new[]{typeof(AnimationModeDriver)}, null);
                if(startAnimationMode != null) startAnimationMode.Invoke(null, new object[]{driver});
                else AnimationMode.StartAnimationMode();
            }

            private static void StopAnimationMode(AnimationModeDriver driver)
            {
                if(stopAnimationMode == null)
                    stopAnimationMode = typeof(AnimationMode).GetMethod("StopAnimationMode", BindingFlags.Static | BindingFlags.NonPublic, null, new[]{typeof(AnimationModeDriver)}, null);
                if(stopAnimationMode != null) stopAnimationMode.Invoke(null, new object[]{driver});
                else AnimationMode.StopAnimationMode();
            }
        #endif

        private void StartPreview(ParametersPerMenu parameter)
        {
            if(parameter == null || parameter.objects == null || parameter.blendShapeModifiers == null || parameter.materialReplacers == null || parameter.materialPropertyModifiers == null) return;
            m_Parameters = parameter;
            StartAnimationMode(Driver);
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            SampleParameters();
        }

        private void StartPreview(ParametersPerMenu[] parameters)
        {
            var def = parameters.CreateDefaultParameters();
            StartPreview(parameters[previewIndex].Merge(def));
        }

        private void StartPreview(ItemToggler toggler)
        {
            StartPreview(toggler.parameter);
        }

        private void StartPreview(CostumeChanger changer)
        {
            if(previewIndex >= changer.costumes.Length) return;
            StartPreview(changer.costumes.Select(c => c.parametersPerMenu).ToArray());
        }

        private void StartPreview(SmoothChanger changer)
        {
            if(previewIndex >= changer.frames.Length) return;
            StartPreview(changer.frames.Select(c => c.parametersPerMenu).ToArray());
        }

        internal void StartPreview(Object obj)
        {
            if(!obj || target == obj || !doPreview || AnimationMode.InAnimationMode() || !((Component)obj).gameObject.GetAvatarRoot()) return;
            target = (Component)obj;
            switch(obj)
            {
                case ItemToggler c: if(c.parameter != null) StartPreview(c); break;
                case CostumeChanger c: if(c.costumes != null) StartPreview(c); break;
                case SmoothChanger c: if(c.frames != null) StartPreview(c); break;
            }
        }

        internal void StopPreview()
        {
            StopAnimationMode(Driver);
            EditorApplication.update -= Update;
            target = null;
        }

        private void Update()
        {
            if(!target) StopPreview();
        }

        internal bool ChechTargetHasPreview(Object obj)
        {
            if(!obj) return false;
            switch(obj)
            {
                case ItemToggler _: return true;
                case CostumeChanger _: return true;
                case SmoothChanger _: return true;
            }
            return false;
        }

        internal void TogglePreview()
        {
            EditorGUI.BeginChangeCheck();
            doPreview = GUILayout.Toolbar(doPreview ? 0 : 1, new[]{Localization.G("inspector.preview"), Localization.G("inspector.previewStop")}) == 0;
            if(EditorGUI.EndChangeCheck() && !doPreview) StopPreview();
        }

        private void DrawIndex(int size, string key)
        {
            EditorGUI.BeginChangeCheck();
            previewIndex = EditorGUILayout.IntSlider(Localization.G(key), previewIndex, 0, size - 1);
            if(EditorGUI.EndChangeCheck()) StopPreview();
        }

        internal void DrawIndex(Object obj)
        {
            if(!obj) return;
            switch(obj)
            {
                case CostumeChanger c: if(c.costumes != null) DrawIndex(c.costumes.Length, "inspector.previewCostume"); return;
                case SmoothChanger c: if(c.frames != null) DrawIndex(c.frames.Length, "inspector.previewFrame"); break;
            }
        }

        internal void SampleParameters()
        {
            var gameObject = target.gameObject.GetAvatarRoot().gameObject;
            foreach(var toggler in m_Parameters.objects)
            {
                if(!toggler.obj) continue;
                var binding = AnimationHelper.CreateToggleBinding(toggler.obj);
                AnimationMode.AddPropertyModification(binding, new PropertyModification{
                    propertyPath = new SerializedObject(toggler.obj).FindProperty("m_IsActive").propertyPath,
                    target = toggler.obj,
                    value = toggler.obj.activeSelf ? "1" : "0"
                }, true);
                toggler.obj.SetActive(toggler.value);
            }

            foreach(var modifier in m_Parameters.blendShapeModifiers)
            {
                if(modifier.applyToAll)
                {
                    var renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach(var renderer in renderers)
                    {
                        if(!renderer || !renderer.sharedMesh) continue;
                        foreach(var namevalue in modifier.blendShapeNameValues)
                        {
                            var index = renderer.sharedMesh.GetBlendShapeIndex(namevalue.name);
                            if(index == -1) continue;
                            var binding = AnimationHelper.CreateBlendShapeBinding(renderer, namevalue.name);
                            AnimationMode.AddPropertyModification(binding, new PropertyModification{
                                propertyPath = new SerializedObject(renderer).FindProperty("m_BlendShapeWeights").GetArrayElementAtIndex(index).propertyPath,
                                target = renderer,
                                value = renderer.GetBlendShapeWeight(index).ToString(CultureInfo.InvariantCulture)
                            }, true);
                            renderer.SetBlendShapeWeight(index, namevalue.value);
                        }
                    }
                    continue;
                }
                if(!modifier.skinnedMeshRenderer) continue;
                foreach(var namevalue in modifier.blendShapeNameValues)
                {
                    var renderer = modifier.skinnedMeshRenderer;
                    var index = renderer.sharedMesh.GetBlendShapeIndex(namevalue.name);
                    if(index == -1) continue;
                    var binding = AnimationHelper.CreateBlendShapeBinding(renderer, namevalue.name);
                    AnimationMode.AddPropertyModification(binding, new PropertyModification{
                        propertyPath = new SerializedObject(renderer).FindProperty("m_BlendShapeWeights").GetArrayElementAtIndex(index).propertyPath,
                        target = renderer,
                        value = renderer.GetBlendShapeWeight(index).ToString(CultureInfo.InvariantCulture)
                    }, true);
                    renderer.SetBlendShapeWeight(index, namevalue.value);
                }
            }

            foreach(var replacer in m_Parameters.materialReplacers)
            {
                if(!replacer.renderer) continue;
                var materials = replacer.renderer.sharedMaterials;
                var modified = false;
                for(int i = 0; i < replacer.replaceTo.Length; i++)
                {
                    if(!replacer.replaceTo[i]) continue;
                    var binding = AnimationHelper.CreateMaterialReplaceBinding(replacer.renderer, i);
                    AnimationMode.AddPropertyModification(binding, new PropertyModification{
                        propertyPath = new SerializedObject(replacer.renderer).FindProperty("m_Materials").GetArrayElementAtIndex(i).propertyPath,
                        target = replacer.renderer,
                        objectReference = replacer.renderer.sharedMaterials[i]
                    }, true);
                    materials[i] = replacer.replaceTo[i];
                    modified = true;
                }
                if(modified) replacer.renderer.sharedMaterials = materials;
            }

            // MaterialPropertyModifier as replacer
            foreach(var modifier in m_Parameters.materialPropertyModifiers)
            {
                var renderers = modifier.renderers;
                if(renderers.Length == 0)
                    renderers = gameObject.GetComponentsInChildren<Renderer>(false).ToArray(); // Slows down when there are a large number of costumes, so false 

                foreach(var renderer in renderers)
                {
                    if(!renderer) continue;
                    var materials = renderer.sharedMaterials;
                    for(int i = 0; i < materials.Length; i++)
                    {
                        if(!materials[i]) continue;
                        var binding = AnimationHelper.CreateMaterialReplaceBinding(renderer, i);
                        AnimationMode.AddPropertyModification(binding, new PropertyModification{
                            propertyPath = new SerializedObject(renderer).FindProperty("m_Materials").GetArrayElementAtIndex(i).propertyPath,
                            target = renderer,
                            objectReference = renderer.sharedMaterials[i]
                        }, true);

                        var material = new Material(materials[i]);
                        foreach(var floatModifier in modifier.floatModifiers)
                            material.SetFloat(floatModifier.propertyName, floatModifier.value);
                        foreach(var vectorModifier in modifier.vectorModifiers)
                            material.SetVector(vectorModifier.propertyName, vectorModifier.value);
                        
                        materials[i] = material;
                    }
                    renderer.sharedMaterials = materials;
                }
            }
        }
    }
}
