using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    // Unity標準のAnimationClipのかわり
    // 1フレームしか扱えないが高速で処理できる
    internal class InternalClip
    {
        internal Dictionary<EditorCurveBinding,(float,Object,bool)> bindings = new Dictionary<EditorCurveBinding,(float,Object,bool)>();
        internal string name = "";

        internal AnimationClip ToClip()
        {
            var clip = new AnimationClip{name = name};
            foreach(var b in bindings)
            {
                if(b.Value.Item3)
                {
                    var curve = new[]{new ObjectReferenceKeyframe{time = 0, value = b.Value.Item2}};
                    AnimationUtility.SetObjectReferenceCurve(clip, b.Key, curve);
                }
                else
                {
                    var curve = new AnimationCurve();
                    curve.AddKey(0, b.Value.Item1);
                    AnimationUtility.SetEditorCurve(clip, b.Key, curve);
                }
            }
            return clip;
        }

        internal void Add(EditorCurveBinding binding, bool value) => bindings[binding] = (value ? 1 : 0, null, false);
        internal void Add(EditorCurveBinding binding, float value) => bindings[binding] = (value, null, false);
        internal void Add(EditorCurveBinding binding, Object value) => bindings[binding] = (0, value, true);
        internal void Add(AnimationClip clip)
        {
            foreach(var binding in AnimationUtility.GetCurveBindings(clip))
                Add(binding, AnimationUtility.GetEditorCurve(clip, binding).keys[0].value);
            foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                Add(binding, AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value);
        }
        internal void AddDefault(AnimationClip clip, GameObject root)
        {
            foreach(var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var obj = AnimationUtility.GetAnimatedObject(root, binding);
                if(obj)
                {
                    var prop = new SerializedObject(obj).FindProperty(binding.propertyName);
                    if(prop != null)
                    {
                        switch(prop.propertyType)
                        {
                            case SerializedPropertyType.Integer: Add(binding, prop.intValue); continue;
                            case SerializedPropertyType.Boolean: Add(binding, prop.boolValue); continue;
                            case SerializedPropertyType.Float: Add(binding, prop.floatValue); continue;
                            case SerializedPropertyType.Color:
                                if(binding.propertyName.EndsWith(".r"))Add(binding, prop.colorValue.r);
                                else if(binding.propertyName.EndsWith(".g"))Add(binding, prop.colorValue.g);
                                else if(binding.propertyName.EndsWith(".b"))Add(binding, prop.colorValue.b);
                                else if(binding.propertyName.EndsWith(".a"))Add(binding, prop.colorValue.a);
                                continue;
                            case SerializedPropertyType.Vector2:
                                if(binding.propertyName.EndsWith(".x"))Add(binding, prop.vector2Value.x);
                                else if(binding.propertyName.EndsWith(".y"))Add(binding, prop.vector2Value.y);
                                continue;
                            case SerializedPropertyType.Vector3:
                                if(binding.propertyName.EndsWith(".x"))Add(binding, prop.vector3Value.x);
                                else if(binding.propertyName.EndsWith(".y"))Add(binding, prop.vector3Value.y);
                                else if(binding.propertyName.EndsWith(".z"))Add(binding, prop.vector3Value.z);
                                continue;
                            case SerializedPropertyType.Vector4:
                                if(binding.propertyName.EndsWith(".x"))Add(binding, prop.vector4Value.x);
                                else if(binding.propertyName.EndsWith(".y"))Add(binding, prop.vector4Value.y);
                                else if(binding.propertyName.EndsWith(".z"))Add(binding, prop.vector4Value.z);
                                else if(binding.propertyName.EndsWith(".w"))Add(binding, prop.vector4Value.w);
                                continue;
                            case SerializedPropertyType.Vector2Int:
                                if(binding.propertyName.EndsWith(".x"))Add(binding, prop.vector2IntValue.x);
                                else if(binding.propertyName.EndsWith(".y"))Add(binding, prop.vector2IntValue.y);
                                continue;
                            case SerializedPropertyType.Vector3Int:
                                if(binding.propertyName.EndsWith(".x"))Add(binding, prop.vector3IntValue.x);
                                else if(binding.propertyName.EndsWith(".y"))Add(binding, prop.vector3IntValue.y);
                                else if(binding.propertyName.EndsWith(".z"))Add(binding, prop.vector3IntValue.z);
                                continue;
                        }
                    }
                }
                Add(binding, AnimationUtility.GetEditorCurve(clip, binding).keys[0].value);
            }
            foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                var obj = AnimationUtility.GetAnimatedObject(root, binding);
                if(obj)
                {
                    var prop = new SerializedObject(obj).FindProperty(binding.propertyName);
                    if(prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        Add(binding, prop.objectReferenceValue);
                        continue;
                    }
                }
                Add(binding, AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value);
            }
        }

        internal void Merge(InternalClip src, bool overwrite = false)
        {
            if(overwrite)
            {
                foreach(var b in src.bindings)
                {
                    bindings[b.Key] = b.Value;
                }
            }
            else
            {
                foreach(var b in src.bindings)
                {
                    if(bindings.ContainsKey(b.Key)) continue;
                    bindings[b.Key] = b.Value;
                }
            }
        }

        internal void Merge(InternalClip[] srcs, bool overwrite = false)
        {
            foreach(var src in srcs) Merge(src,overwrite);
        }

        internal static InternalClip MergeAndCreate(InternalClip[] srcs)
        {
            var newClip = new InternalClip();
            foreach(var src in srcs) newClip.Merge(src);
            return newClip;
        }

        internal static InternalClip MergeAndCreate(InternalClip src, InternalClip def)
        {
            var newClip = new InternalClip();
            newClip.Merge(src);
            newClip.Merge(def);
            return newClip;
        }
    }
}
