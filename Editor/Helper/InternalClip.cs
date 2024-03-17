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
