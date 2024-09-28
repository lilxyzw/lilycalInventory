using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ResolveMultiConditions(AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers, CostumeChanger[] costumeChangers, BlendTree root)
            {
                // 複数コンポーネントから操作されるオブジェクトを見つける
                var toggleBools = new Dictionary<GameObject, HashSet<(string name, bool toActive, bool defaultValue)>>();
                var toggleInts = new Dictionary<GameObject, HashSet<(string name, bool[] toActives, int defaultValue)>>();
                var shapeBools = new Dictionary<(SkinnedMeshRenderer, string), HashSet<(string name, bool toActive, bool defaultValue)>>();
                var shapeInts = new Dictionary<(SkinnedMeshRenderer, string), HashSet<(string name, bool[] toActives, int defaultValue)>>();
                togglers.GatherConditions(toggleBools, shapeBools);
                costumeChangers.GatherConditions(toggleInts, shapeInts);

                // オブジェクトのオンオフ
                var multiConditionObjects = toggleBools.Keys.Concat(toggleInts.Keys)
                    .Distinct()
                    .Where(o => (toggleBools.TryGetValue(o, out var b) ? b.Count() : 0) + (toggleInts.TryGetValue(o, out var i) ? i.Count() : 0) > 1)
                    .ToArray();

                // BlendShapeの操作
                var multiConditionShapes = shapeBools.Keys.Concat(shapeInts.Keys)
                    .Distinct()
                    .Where(o => (shapeBools.TryGetValue(o, out var b) ? b.Count() : 0) + (shapeInts.TryGetValue(o, out var i) ? i.Count() : 0) > 1)
                    .ToArray();

                // 各コンポーネントからそのオブジェクトを除去
                foreach(var t in togglers)
                {
                    t.parameter.objects = t.parameter.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();
                    foreach(var bsModifier in t.parameter.blendShapeModifiers)
                    {
                        bsModifier.blendShapeNameValues = bsModifier.blendShapeNameValues
                            .Where(o => !multiConditionShapes.Contains((bsModifier.skinnedMeshRenderer, o.name))).ToArray();
                    }
                }
                foreach(var c in costumeChangers)
                foreach(var t in c.costumes)
                {
                    t.parametersPerMenu.objects = t.parametersPerMenu.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();
                    foreach(var bsModifier in t.parametersPerMenu.blendShapeModifiers)
                    {
                        bsModifier.blendShapeNameValues = bsModifier.blendShapeNameValues
                            .Where(o => !multiConditionShapes.Contains((bsModifier.skinnedMeshRenderer, o.name))).ToArray();
                    }
                }

                // 同一条件のオブジェクトをまとめる
                var conditionAndObjects = new Dictionary<((string name, bool toActive, bool defaultValue)[] bools, (string name, bool[] toActives, int defaultValue)[] ints, bool isActive), List<ConditionObject>>();
                foreach(var o in multiConditionObjects)
                {
                    var bools = toggleBools.ContainsKey(o) ? toggleBools[o].OrderBy(b => b.name).ToArray() : new (string name, bool toActive, bool defaultValue)[0];
                    var ints = toggleInts.ContainsKey(o) ? toggleInts[o].OrderBy(i => i.name).ToArray() : new (string name, bool[] toActives, int defaultValue)[0];
                    var isActive = o.activeSelf;
                    var key = conditionAndObjects.Keys.Where(x => IsSameConditions(x, (bools, ints, isActive))).DefaultIfEmpty((bools, ints, isActive)).Single();
                    if(conditionAndObjects.ContainsKey(key))
                    {
                        conditionAndObjects[key].Add(new ConditionObject(){type=ConditionObjectType.ObjectToggler,obj=o});
                    }
                    else
                    {
                        conditionAndObjects[key] = new List<ConditionObject>{new(){type=ConditionObjectType.ObjectToggler,obj=o}};
                    }
                }

                // 同一条件のオブジェクトをまとめる
                foreach(var o in multiConditionShapes)
                {
                    var bools = shapeBools.ContainsKey(o) ? shapeBools[o].OrderBy(b => b.name).ToArray() : new (string name, bool toActive, bool defaultValue)[0];
                    var ints = shapeInts.ContainsKey(o) ? shapeInts[o].OrderBy(i => i.name).ToArray() : new (string name, bool[] toActives, int defaultValue)[0];
                    var isActive = o.Item1.GetBlendShapeWeight(o.Item2) == 100;
                    var key = conditionAndObjects.Keys.Where(x => IsSameConditions(x, (bools, ints, isActive))).DefaultIfEmpty((bools, ints, isActive)).Single();
                    if(conditionAndObjects.ContainsKey(key))
                    {
                        conditionAndObjects[key].Add(new ConditionObject(){type=ConditionObjectType.BlendShapeModifier,obj=o.Item1,blendShape=o.Item2});
                    }
                    else
                    {
                        conditionAndObjects[key] = new List<ConditionObject>{new(){type=ConditionObjectType.BlendShapeModifier,obj=o.Item1,blendShape=o.Item2}};
                    }
                }

                // アニメーションの生成
                foreach(var c in conditionAndObjects)
                {
                    var bools = c.Key.bools;
                    var ints = c.Key.ints;
                    var isActive = c.Key.isActive;

                    var name = c.Value.ElementAt(0).obj.name;
                    var clips = (clipDefault: new InternalClip(), clipChanged: new InternalClip());
                    clips.clipDefault.name = $"{name}_Default";
                    clips.clipChanged.name = $"{name}_Changed";
                    foreach(var o in c.Value)
                    {
                        if(o.type == ConditionObjectType.ObjectToggler)
                        {
                            var toggler = new ObjectToggler
                            {
                                obj = o.obj as GameObject,
                                value = !isActive
                            };
                            toggler.ToClipDefault(clips.clipDefault);
                            toggler.ToClip(clips.clipChanged);
                        }
                        else if(o.type == ConditionObjectType.BlendShapeModifier)
                        {
                            var nameValue = new BlendShapeNameValue(){name = o.blendShape, value = !isActive ? 100 : 0};
                            var smr = o.obj as SkinnedMeshRenderer;
                            nameValue.ToClipDefault(clips.clipDefault, smr);
                            nameValue.ToClip(clips.clipChanged, smr);
                        }
                    }
                    var clipDefault = clips.clipDefault.ToClip();
                    var clipChanged = clips.clipChanged.ToClip();

                    AssetDatabase.AddObjectToAsset(clipDefault, ctx.AssetContainer);
                    AssetDatabase.AddObjectToAsset(clipChanged, ctx.AssetContainer);
                    if(root) AnimationHelper.AddMultiConditionTree(controller, clipDefault, clipChanged, bools, ints, root, isActive);
                    else AnimationHelper.AddMultiConditionLayer(controller, hasWriteDefaultsState, clipDefault, clipChanged, name, bools, ints, isActive);
                }
            }

            private static bool IsSameConditions(((string name, bool toActive, bool defaultValue)[] bools, (string name, bool[] toActives, int defaultValue)[] ints, bool isActive) a, ((string name, bool toActive, bool defaultValue)[] bools, (string name, bool[] toActives, int defaultValue)[] ints, bool isActive) b)
            {
                if(!a.bools.SequenceEqual(b.bools)) return false;
                if(!a.ints.Select(k => k.name).SequenceEqual(b.ints.Select(k => k.name))) return false;
                for(int i = 0; i < a.ints.Length; i++)
                {
                    if(!a.ints[i].toActives.SequenceEqual(b.ints[i].toActives)) return false;
                }
                if(!a.ints.Select(k => k.defaultValue).SequenceEqual(b.ints.Select(k => k.defaultValue))) return false;
                return a.isActive == b.isActive;
            }

            internal class ConditionObject
            {
                public ConditionObjectType type;
                public Object obj;
                public string blendShape;
            }

            internal enum ConditionObjectType
            {
                ObjectToggler,
                BlendShapeModifier
            }
        }
    }
}
