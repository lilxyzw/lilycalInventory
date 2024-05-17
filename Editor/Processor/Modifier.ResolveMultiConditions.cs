using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal partial class Modifier
    {
        internal static void ResolveMultiConditions(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers, CostumeChanger[] costumeChangers, BlendTree root)
        {
            // 複数コンポーネントから操作されるオブジェクトを見つける
            var toggleBools = new Dictionary<GameObject, HashSet<(string name, bool toActive)>>();
            var toggleInts = new Dictionary<GameObject, HashSet<(string name, bool[] toActives)>>();
            togglers.GatherConditions(toggleBools);
            costumeChangers.GatherConditions(toggleInts);
            var multiConditionObjects = toggleBools.Keys.Concat(toggleInts.Keys)
                .Distinct()
                .Where(o => toggleBools.TryGetValue(o, out var b) && b.Any() || toggleInts.TryGetValue(o, out var i) && i.Any())
                .ToArray();

            // 各コンポーネントからそのオブジェクトを除去
            foreach(var t in togglers)
                t.parameter.objects = t.parameter.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();
            foreach(var c in costumeChangers)
                foreach(var t in c.costumes)
                    t.parametersPerMenu.objects = t.parametersPerMenu.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();

            // 同一条件のオブジェクトをまとめる
            var conditionAndObjects = new Dictionary<((string name, bool toActive)[] bools, (string name, bool[] toActives)[] ints, bool isActive), List<GameObject>>();
            foreach(var o in multiConditionObjects)
            {
                var bools = toggleBools.ContainsKey(o) ? toggleBools[o].OrderBy(b => b.name).ToArray() : new (string name, bool toActive)[0];
                var ints = toggleInts.ContainsKey(o) ? toggleInts[o].OrderBy(i => i.name).ToArray() : new (string name, bool[] toActives)[0];
                var isActive = o.activeSelf;
                var key = conditionAndObjects.Keys.Where(x => IsSameConditions(x, (bools, ints, isActive))).DefaultIfEmpty((bools, ints, isActive)).Single();
                if(conditionAndObjects.ContainsKey(key))
                {
                    conditionAndObjects[key].Add(o);
                }
                else
                {
                    conditionAndObjects[key] = new List<GameObject>{o};
                }
            }

            // アニメーションの生成
            foreach(var c in conditionAndObjects)
            {
                var bools = c.Key.bools;
                var ints = c.Key.ints;
                var isActive = c.Key.isActive;

                var name = c.Value.ElementAt(0).name;
                var clips = (clipDefault: new InternalClip(), clipChanged: new InternalClip());
                clips.clipDefault.name = $"{name}_Default";
                clips.clipChanged.name = $"{name}_Changed";
                foreach(var o in c.Value)
                {
                    var toggler = new ObjectToggler
                    {
                        obj = o,
                        value = !isActive
                    };
                    toggler.ToClipDefault(clips.clipDefault);
                    toggler.ToClip(clips.clipChanged);
                }
                var clipDefault = clips.clipDefault.ToClip();
                var clipChanged = clips.clipChanged.ToClip();

                AssetDatabase.AddObjectToAsset(clipDefault, ctx.AssetContainer);
                AssetDatabase.AddObjectToAsset(clipChanged, ctx.AssetContainer);
                if(root) AnimationHelper.AddMultiConditionTree(controller, clipDefault, clipChanged, bools, ints, root, isActive);
                else AnimationHelper.AddMultiConditionLayer(controller, hasWriteDefaultsState, clipDefault, clipChanged, name, bools, ints, isActive);
            }
        }

        private static bool IsSameConditions(((string name, bool toActive)[] bools, (string name, bool[] toActives)[] ints, bool isActive) a, ((string name, bool toActive)[] bools, (string name, bool[] toActives)[] ints, bool isActive) b)
        {
            if(!a.bools.SequenceEqual(b.bools)) return false;
            if(!a.ints.Select(k => k.name).SequenceEqual(b.ints.Select(k => k.name))) return false;
            for(int i = 0; i < a.ints.Length; i++)
            {
                if(!a.ints[i].toActives.SequenceEqual(b.ints[i].toActives)) return false;
            }
            return a.isActive == b.isActive;
        }
    }
}
