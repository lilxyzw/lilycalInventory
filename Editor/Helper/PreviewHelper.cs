using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Globalization;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal class PreviewHelper : ScriptableSingleton<PreviewHelper>
    {
        internal static int doPreview = 0; // Off, On, Always
        private static Component target;
        private static ParametersPerMenu m_Parameters;
        private static int previewIndex = 0;
        private static float previewFrame = 0;

        // オブジェクト変更状況を外部から辿れるようにする
        private static readonly Dictionary<Object, Dictionary<string, object>> valueContainer = new();

        [SerializeField] private AnimationModeDriver driver;
        private AnimationModeDriver Driver => driver ? driver : driver = CreateDriver();
        private AnimationModeDriver CreateDriver() => CreateInstance<AnimationModeDriver>();
        private static void StartAnimationMode(AnimationModeDriver driver) => AnimationMode.StartAnimationMode(driver);
        private static void StopAnimationMode(AnimationModeDriver driver) => AnimationMode.StopAnimationMode(driver);

        private void StartPreview(ParametersPerMenu parameter)
        {
            if(parameter == null || parameter.objects == null || parameter.blendShapeModifiers == null || parameter.materialReplacers == null || parameter.materialPropertyModifiers == null) return;
            m_Parameters = parameter;
            StartAnimationMode(Driver);
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            SampleParameters();
        }

        private void StartPreview(ParametersPerMenu[] parameters, int index)
        {
            var def = parameters.CreateDefaultParameters();
            StartPreview(parameters[index].Merge(def));
        }

        private void StartPreview(ParametersPerMenu[] parameters) => StartPreview(parameters, previewIndex);

        private void StartPreview(ItemToggler toggler)
        {
            StartPreview(toggler.parameter);
        }

        private void StartPreview(Prop prop)
        {
            StartPreview(prop.PropToTogglerParameters());
        }

        private void StartPreview(CostumeChanger changer)
        {
            if(previewIndex >= changer.costumes.Length) return;
            StartPreview(changer.costumes.Select(c => c.parametersPerMenu).ToArray());
        }

        private void StartPreview(SmoothChanger changer)
        {
            if(previewIndex >= changer.frames.Length) return;
            StartPreview(Interpolate(changer.frames));
        }

        private void StartPreview(AutoDresser dresser)
        {
            var gameObject = target.gameObject.GetAvatarRoot().gameObject;

            var dressers = gameObject.GetComponentsInChildren<AutoDresser>(true).Where(c => c.enabled).ToArray();
            var parameters = dressers.DresserToCostumes(out Transform avatarRoot, null, new Preset[]{}, dresser).Select(c => c.parametersPerMenu).ToArray();

            StartPreview(parameters, 0);
        }

        internal void StartPreview(Object obj)
        {
            if(!obj || target == obj || doPreview == 0 || AnimationMode.InAnimationMode() || !((Component)obj).gameObject.GetAvatarRoot() || !((Component)obj).gameObject.scene.IsValid()) return;
            target = (Component)obj;
            switch(obj)
            {
                case Prop c: if(c.parameter != null) StartPreview(c); break;
                case ItemToggler c: if(c.parameter != null) StartPreview(c); break;
                case CostumeChanger c: if(c.costumes != null) StartPreview(c); break;
                case SmoothChanger c: if(c.frames != null) StartPreview(c); break;
                case AutoDresser c: if(c.parameter != null) StartPreview(c); break;
            }
        }

        internal void StopPreview()
        {
            StopAnimationMode(Driver);
            valueContainer.Clear();
            EditorApplication.update -= Update;
            target = null;
        }

        private void Update()
        {
            if(!target) StopPreview();
        }

        internal bool ChechTargetHasPreview(Object obj)
        {
            if(!obj || !((Component)obj).gameObject.scene.IsValid()) return false;
            switch(obj)
            {
                case Prop _:
                case ItemToggler _:
                case CostumeChanger _:
                case SmoothChanger _:
                case AutoDresser _:
                    return true;
            }
            return false;
        }

        // プレビュー切り替え用のGUI
        internal bool m_IsActive = false;
        internal bool IsActive(Object obj) => Event.current == null || Event.current.type != EventType.Layout ? m_IsActive : m_IsActive = AnimationMode.IsPropertyAnimated(((Component)obj).gameObject, "m_IsActive"); 
        internal void TogglePreview(Object obj)
        {
            var rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), Localization.G("inspector.previewAnimation"));
            EditorGUI.BeginChangeCheck();
            doPreview = GUI.Toolbar(rect, doPreview, new[]{Localization.G("inspector.previewStop"), Localization.G("inspector.preview"), Localization.G("inspector.previewAlways")});
            var isChanged = EditorGUI.EndChangeCheck();

            // 選択しているオブジェクト自体がアニメーションされている場合は警告を表示
            if(doPreview != 0 && IsActive(obj))
                EditorGUILayout.HelpBox(Localization.S("inspector.previewWarn"), MessageType.Warning);

            if(isChanged && doPreview == 0) StopPreview();
        }

        // プレビュー衣装切り替え用のGUI
        private void DrawIndex(int size, string key)
        {
            EditorGUI.BeginDisabledGroup(size == 0);
            EditorGUI.BeginChangeCheck();
            previewIndex = EditorGUILayout.IntSlider(Localization.G(key), previewIndex, 0, size - 1);
            if(EditorGUI.EndChangeCheck()) StopPreview();
            EditorGUI.EndDisabledGroup();

            if(previewIndex < 0) previewIndex = 0;
        }

        // プレビューフレーム切り替え用のGUI
        private void DrawIndex(string key)
        {
            EditorGUI.BeginChangeCheck();
            previewFrame = EditorGUILayout.Slider(Localization.G(key), previewFrame * 100f, 0f, 100f) / 100f;
            if(EditorGUI.EndChangeCheck()) StopPreview();
        }

        internal void DrawIndex(Object obj)
        {
            if(!obj) return;
            switch(obj)
            {
                case CostumeChanger c: if(c.costumes != null) DrawIndex(c.costumes.Length, "inspector.previewCostume"); return;
                case SmoothChanger c: if(c.frames != null) DrawIndex("inspector.previewFrame"); break;
            }
        }

        // ParametersPerMenuから設定を読み取って実際にアニメーションさせる
        internal void SampleParameters()
        {
            var gameObject = target.gameObject.GetAvatarRoot().gameObject;
            foreach(var toggler in m_Parameters.objects)
            {
                if(!toggler.obj) continue;
                var binding = AnimationHelper.CreateToggleBinding(toggler.obj);
                using var so = new SerializedObject(toggler.obj);
                using var m_IsActive = so.FindProperty("m_IsActive");
                AddPropertyModification(binding, toggler.obj, m_IsActive.propertyPath, toggler.obj.activeSelf);
                toggler.obj.SetActive(toggler.value);
            }

            foreach(var modifier in m_Parameters.blendShapeModifiers)
            {
                if(!modifier.skinnedMeshRenderer)
                {
                    var renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach(var renderer in renderers)
                    {
                        if(!renderer || !renderer.sharedMesh) continue;
                        foreach(var namevalue in modifier.blendShapeNameValues)
                        {
                            var index = renderer.sharedMesh.GetBlendShapeIndex(namevalue.name);
                            if(index == -1) continue;
                            using var so = new SerializedObject(renderer);
                            var m_BlendShapeWeights = so.FindProperty("m_BlendShapeWeights");
                            if(index >= m_BlendShapeWeights.arraySize) continue;
                            var binding = AnimationHelper.CreateBlendShapeBinding(renderer, namevalue.name);
                            using var element = m_BlendShapeWeights.GetArrayElementAtIndex(index);
                            AddPropertyModification(binding, renderer, element.propertyPath, renderer.GetBlendShapeWeight(index));
                            renderer.SetBlendShapeWeight(index, namevalue.value);
                        }
                    }
                    continue;
                }
                foreach(var namevalue in modifier.blendShapeNameValues)
                {
                    var renderer = modifier.skinnedMeshRenderer;
                    var index = renderer.sharedMesh.GetBlendShapeIndex(namevalue.name);
                    if(index == -1) continue;
                    var binding = AnimationHelper.CreateBlendShapeBinding(renderer, namevalue.name);
                    using var so = new SerializedObject(renderer);
                    using var m_BlendShapeWeights = so.FindProperty("m_BlendShapeWeights");
                    using var element = m_BlendShapeWeights.GetArrayElementAtIndex(index);
                    AddPropertyModification(binding, renderer, element.propertyPath, renderer.GetBlendShapeWeight(index));
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
                    using var so = new SerializedObject(replacer.renderer);
                    using var m_BlendShapeWeights = so.FindProperty("m_Materials");
                    using var element = m_BlendShapeWeights.GetArrayElementAtIndex(i);
                    AddPropertyModification(binding, replacer.renderer, element.propertyPath, replacer.renderer.sharedMaterials[i]);
                    materials[i] = replacer.replaceTo[i];
                    modified = true;
                }
                if(modified) replacer.renderer.sharedMaterials = materials;
            }

            // MaterialPropertyModifierはマテリアル置き換えで動作
            foreach(var modifier in m_Parameters.materialPropertyModifiers)
            {
                var renderers = modifier.renderers;
                if(renderers.Length == 0)
                    renderers = gameObject.GetComponentsInChildren<Renderer>(false).ToArray(); // 大量の衣装がある場合に遅くなるので非表示のメッシュは無視

                foreach(var renderer in renderers)
                {
                    if(!renderer) continue;
                    var materials = renderer.sharedMaterials;
                    for(int i = 0; i < materials.Length; i++)
                    {
                        if(!materials[i]) continue;
                        var binding = AnimationHelper.CreateMaterialReplaceBinding(renderer, i);
                        using var so = new SerializedObject(renderer);
                        using var m_BlendShapeWeights = so.FindProperty("m_Materials");
                        using var element = m_BlendShapeWeights.GetArrayElementAtIndex(i);
                        AddPropertyModification(binding, renderer, element.propertyPath, renderer.sharedMaterials[i]);
                        var material = new Material(materials[i]);
                        foreach(var floatModifier in modifier.floatModifiers)
                        {
                            if(!material.HasProperty(floatModifier.propertyName)) continue;
                            material.SetFloat(floatModifier.propertyName, floatModifier.value);
                        }
                        foreach(var vectorModifier in modifier.vectorModifiers)
                        {
                            if(!material.HasProperty(vectorModifier.propertyName)) continue;
                            var value = material.GetVector(vectorModifier.propertyName);
                            if(!vectorModifier.disableX) value.x = vectorModifier.value.x;
                            if(!vectorModifier.disableY) value.y = vectorModifier.value.y;
                            if(!vectorModifier.disableZ) value.z = vectorModifier.value.z;
                            if(!vectorModifier.disableW) value.w = vectorModifier.value.w;
                            material.SetVector(vectorModifier.propertyName, value);
                        }
                        
                        materials[i] = material;
                    }
                    renderer.sharedMaterials = materials;
                }
            }
        }

        // 外部から変更状況を見れるように同時にDictionaryへの登録も行う
        private static void AddPropertyModification(EditorCurveBinding binding, Object target, string propertyPath, Object value)
        {
            AddToContainer(target, propertyPath, value);
            AnimationMode.AddPropertyModification(binding, new PropertyModification{
                propertyPath = propertyPath,
                target = target,
                objectReference = value
            }, true);
        }

        private static void AddPropertyModification(EditorCurveBinding binding, Object target, string propertyPath, float value)
        {
            AddToContainer(target, propertyPath, value);
            AnimationMode.AddPropertyModification(binding, new PropertyModification{
                propertyPath = propertyPath,
                target = target,
                value = value.ToString(CultureInfo.InvariantCulture)
            }, true);
        }

        private static void AddPropertyModification(EditorCurveBinding binding, Object target, string propertyPath, bool value)
        {
            AddToContainer(target, propertyPath, value);
            AnimationMode.AddPropertyModification(binding, new PropertyModification{
                propertyPath = propertyPath,
                target = target,
                value = value ? "1" : "0"
            }, true);
        }

        private static void AddToContainer(Object target, string propertyPath, object value)
        {
            if(!valueContainer.ContainsKey(target)) valueContainer[target] = new Dictionary<string, object>();
            valueContainer[target][propertyPath] = value;
        }

        internal static object GetFromContainer(Object target, string propertyPath)
        {
            if(!valueContainer.ContainsKey(target)) return null;
            if(!valueContainer[target].ContainsKey(propertyPath)) return null;
            return valueContainer[target][propertyPath];
        }

        // SmoothChangerで使用するフレーム間の補間
        private ParametersPerMenu Interpolate(Frame[] frames)
        {
            if(frames.Length == 0) return null;

            var parameters = frames.Select(f => f.parametersPerMenu).ToArray().CreateDefaultParameters();

            if(frames.Length == 1) return frames[0].parametersPerMenu.Merge(parameters);
            var same = frames.Where(f => f.frameValue == previewFrame);
            if(same.Count() > 0) return same.ElementAt(0).parametersPerMenu.Merge(parameters);

            Frame nearestMin = null;
            Frame nearestMax = null;
            float diffMin = -2;
            float diffMax = 2;
            foreach(var f in frames)
            {
                var diff = f.frameValue - previewFrame;
                if(diff < 0 && diff > diffMin)
                {
                    diffMin = diff;
                    nearestMin = f;
                }
                if(diff > 0 && diff < diffMax)
                {
                    diffMax = diff;
                    nearestMax = f;
                }
            }
            if(nearestMin == null) return nearestMax.parametersPerMenu.Merge(parameters);
            if(nearestMax == null) return nearestMin.parametersPerMenu.Merge(parameters);

            bool isNearMax = Mathf.Abs(diffMin) > diffMax;
            float lerpfactor = -diffMin / (Mathf.Abs(diffMin) + diffMax);

            foreach(var obj in parameters.objects)
            {
                bool min = obj.value;
                bool max = obj.value;
                var mins = nearestMin.parametersPerMenu.objects.Where(o => o.obj == obj.obj).Select(o => o.value);
                if(mins.Count() > 0) min = mins.First();
                var maxs = nearestMax.parametersPerMenu.objects.Where(o => o.obj == obj.obj).Select(o => o.value);
                if(maxs.Count() > 0) max = maxs.First();
                obj.value = isNearMax ? max : min;
            }

            var defaultValues = parameters.blendShapeModifiers.Select(m => (m.skinnedMeshRenderer, m.blendShapeNameValues.ToDictionary(nv => nv.name, nv => nv.value))).Distinct().ToList();
            var renderers = nearestMin.parametersPerMenu.blendShapeModifiers.Select(m => m.skinnedMeshRenderer).Union(
                nearestMax.parametersPerMenu.blendShapeModifiers.Select(m => m.skinnedMeshRenderer)
            ).ToArray();
            parameters.blendShapeModifiers = new BlendShapeModifier[renderers.Length];
            for(int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                var min = new Dictionary<string,float>();
                var max = new Dictionary<string,float>();
                var mins = nearestMin.parametersPerMenu.blendShapeModifiers.Where(o => o.skinnedMeshRenderer == r);
                if(mins.Count() > 0) min = mins.First().blendShapeNameValues.ToDictionary(nv => nv.name, nv => nv.value);
                var maxs = nearestMax.parametersPerMenu.blendShapeModifiers.Where(o => o.skinnedMeshRenderer == r);
                if(maxs.Count() > 0) max = maxs.First().blendShapeNameValues.ToDictionary(nv => nv.name, nv => nv.value);

                var names = min.Keys.Union(max.Keys).Distinct().ToArray();

                parameters.blendShapeModifiers[i] = new BlendShapeModifier
                {
                    blendShapeNameValues = new BlendShapeNameValue[names.Length],
                    applyToAll = !r
                };

                int j = 0;
                foreach(var name in names)
                {
                    float def = 0;
                    if(defaultValues.Any(v => v.Item1 == r) && defaultValues.First(v => v.Item1 == r).Item2.ContainsKey(name)) def = defaultValues.First(v => v.Item1 == r).Item2[name];
                    float minV = def;
                    float maxV = def;
                    if(min.ContainsKey(name)) minV = min[name];
                    if(max.ContainsKey(name)) maxV = max[name];
                    parameters.blendShapeModifiers[i].blendShapeNameValues[j].name = name;
                    parameters.blendShapeModifiers[i].blendShapeNameValues[j].value = Mathf.Lerp(minV, maxV, lerpfactor);
                    j++;
                }
            }

            foreach(var rep in parameters.materialReplacers)
            {
                var min = rep.replaceTo;
                var max = rep.replaceTo;
                var mins = nearestMin.parametersPerMenu.materialReplacers.Where(r => r.renderer == rep.renderer).Select(o => o.replaceTo);
                if(mins.Count() > 0) min = mins.First();
                var maxs = nearestMax.parametersPerMenu.materialReplacers.Where(r => r.renderer == rep.renderer).Select(o => o.replaceTo);
                if(maxs.Count() > 0) max = maxs.First();
                rep.replaceTo = isNearMax ? max : min;
            }

            bool IsSame(Renderer[] a, Renderer[] b)
            {
                if(a.Length != b.Length) return false;
                foreach(var r in a)
                {
                    if(!b.Contains(r)) return false;
                }
                return true;
            }

            foreach(var mod in parameters.materialPropertyModifiers)
            {
                var minF = mod.floatModifiers.ToDictionary(m => m.propertyName, m => m.value);
                var minV = mod.vectorModifiers.ToDictionary(m => m.propertyName, m => m.value);
                var maxF = mod.floatModifiers.ToDictionary(m => m.propertyName, m => m.value);
                var maxV = mod.vectorModifiers.ToDictionary(m => m.propertyName, m => m.value);

                var mins = nearestMin.parametersPerMenu.materialPropertyModifiers.Where(m => IsSame(m.renderers, mod.renderers));
                if(mins.Count() > 0)
                {
                    minF = mins.First().floatModifiers.ToDictionary(m => m.propertyName, m => m.value);
                    minV = mins.First().vectorModifiers.ToDictionary(m => m.propertyName, m => m.value);
                }
                var maxs = nearestMax.parametersPerMenu.materialPropertyModifiers.Where(m => IsSame(m.renderers, mod.renderers));
                if(maxs.Count() > 0)
                {
                    maxF = maxs.First().floatModifiers.ToDictionary(m => m.propertyName, m => m.value);
                    maxV = maxs.First().vectorModifiers.ToDictionary(m => m.propertyName, m => m.value);
                }

                for(int i = 0; i < mod.floatModifiers.Length; i++)
                {
                    var m = mod.floatModifiers[i];
                    var minVal = m.value;
                    var maxVal = m.value;
                    if(minF.ContainsKey(m.propertyName)) minVal = minF[m.propertyName];
                    if(maxF.ContainsKey(m.propertyName)) maxVal = maxF[m.propertyName];
                    mod.floatModifiers[i].value = Mathf.Lerp(minVal, maxVal, lerpfactor);
                }

                for(int i = 0; i < mod.vectorModifiers.Length; i++)
                {
                    var m = mod.vectorModifiers[i];
                    var minVal = m.value;
                    var maxVal = m.value;
                    if(minV.ContainsKey(m.propertyName)) minVal = minV[m.propertyName];
                    if(maxV.ContainsKey(m.propertyName)) maxVal = maxV[m.propertyName];
                    mod.vectorModifiers[i].value = Vector4.Lerp(minVal, maxVal, lerpfactor);
                }
            }

            return parameters;
        }
    }
}
