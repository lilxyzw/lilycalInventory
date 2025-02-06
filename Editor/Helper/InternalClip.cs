using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    // Unity標準のAnimationClipのかわり
    // 1フレームしか扱えないが高速で処理できる
    internal class InternalClip
    {
        internal readonly Dictionary<EditorCurveBinding,(float,Object,bool)> bindings = new();
        internal string name = "";

        internal AnimationClip ToClip()
        {
            var clip = new AnimationClip{name = name};
            var referenceBindings = new List<EditorCurveBinding>();
            var referenceCurves = new List<ObjectReferenceKeyframe[]>();
            var floatBindings = new List<EditorCurveBinding>();
            var floatCurves = new List<AnimationCurve>();
            foreach(var b in bindings)
            {
                if(b.Value.Item3)
                {
                    var curve = new[]{new ObjectReferenceKeyframe{time = 0, value = b.Value.Item2}};
                    referenceBindings.Add(b.Key);
                    referenceCurves.Add(curve);
                }
                else
                {
                    var curve = new AnimationCurve();
                    curve.AddKey(0, b.Value.Item1);
                    floatBindings.Add(b.Key);
                    floatCurves.Add(curve);
                }
            }
            AnimationUtility.SetObjectReferenceCurves(clip, referenceBindings.ToArray(), referenceCurves.ToArray());
            AnimationUtility.SetEditorCurves(clip, floatBindings.ToArray(), floatCurves.ToArray());
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
        internal void AddDefault(AnimationClip clip, GameObject root, GameObject compObj)
        {
            foreach(var binding in AnimationUtility.GetCurveBindings(clip))
            {
                     if(AnimationUtility.GetFloatValue(root, binding, out var data)) Add(binding, data);
                else if(AnimationUtility.GetFloatValue(compObj, binding, out var dataC)) Add(binding, dataC);
                else Add(binding, AnimationUtility.GetEditorCurve(clip, binding).keys[0].value);
            }
            foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                     if(AnimationUtility.GetObjectReferenceValue(root, binding, out var data)) Add(binding, data);
                else if(AnimationUtility.GetObjectReferenceValue(compObj, binding, out var dataC)) Add(binding, dataC);
                else Add(binding, AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value);
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
            newClip.Merge(srcs);
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
