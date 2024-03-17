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
            var toggleBools = new Dictionary<GameObject, HashSet<string>>();
            var toggleInts = new Dictionary<GameObject, Dictionary<string, (int,HashSet<(int,bool)>)>>();
            togglers.GatherConditions(toggleBools);
            costumeChangers.GatherConditions(toggleInts);
            var multiConditionObjects = toggleBools.Select(b => b.Key).Concat(toggleInts.Select(i => i.Key)).GroupBy(o => o).Where(g => g.Count() > 1).Select(g => g.ElementAt(0)).ToArray();

            // 各コンポーネントからそのオブジェクトを除去
            foreach(var t in togglers)
                t.parameter.objects = t.parameter.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();
            foreach(var c in costumeChangers)
                foreach(var t in c.costumes)
                    t.parametersPerMenu.objects = t.parametersPerMenu.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();

            // 同一条件のオブジェクトをまとめる
            var conditionAndObjects = new Dictionary<(string[] bools, (string name, int range,(int value, bool isActive)[])[] ints, bool isActive), List<GameObject>>();
            var conditions = new HashSet<(string[] bools, (string name, int range,(int value, bool isActive)[])[] ints, bool isActive)>();
            foreach(var o in multiConditionObjects)
            {
                var bools = new string[]{};
                if(toggleBools.ContainsKey(o)) bools = toggleBools[o].OrderBy(a => a).ToArray();
                var ints = new (string name, int range,(int value, bool isActive)[])[]{};
                if(toggleInts.ContainsKey(o)) ints = toggleInts[o].Select(b => (b.Key,b.Value.Item1,b.Value.Item2.OrderBy(a => a).ToArray())).OrderBy(a => a.Item1).ToArray();
                var key = (bools,ints,o.activeSelf);
                bool isAdded = false;
                foreach(var c in conditions)
                {
                    if(IsSameConditions(c, key))
                    {
                        conditionAndObjects[c].Add(o);
                        isAdded = true;
                        break;
                    }
                }
                if(!isAdded)
                {
                    conditionAndObjects[key] = new List<GameObject>{o};
                    conditions.Add(key);
                }
            }

            // アニメーションの生成
            foreach(var c in conditionAndObjects)
            {
                var bools = c.Key.bools;
                var ints = c.Key.ints;
                var isActive = c.Key.isActive;

                var name = c.Value.ElementAt(0).name;
                var clipOff = new InternalClip();
                var clipOn = new InternalClip();
                clipOff.name = $"{name}_Off";
                clipOn.name = $"{name}_On";
                foreach(var o in c.Value)
                {
                    var toggler = new ObjectToggler
                    {
                        obj = o,
                        value = !isActive
                    };
                    toggler.ToClipDefault(clipOff);
                    toggler.ToClip(clipOn);
                }
                var clipOff2 = clipOff.ToClip();
                var clipOn2 = clipOn.ToClip();

                AssetDatabase.AddObjectToAsset(clipOff2, ctx.AssetContainer);
                AssetDatabase.AddObjectToAsset(clipOn2, ctx.AssetContainer);
                if(root) AnimationHelper.AddMultiConditionTree(controller, clipOff2, clipOn2, bools, ints, root, isActive);
                else AnimationHelper.AddMultiConditionLayer(controller, hasWriteDefaultsState, clipOff2, clipOn2, name, bools, ints, isActive);
            }
        }

        private static bool IsSameConditions((string[] bools, (string name, int range,(int value, bool isActive)[])[] ints, bool isActive) a, (string[] bools, (string name, int range,(int value, bool isActive)[])[] ints, bool isActive) b)
        {
            if(!a.bools.SequenceEqual(b.bools)) return false;
            if(!a.ints.Select(k => k.name).SequenceEqual(b.ints.Select(k => k.name))) return false;
            if(!a.ints.Select(k => k.range).SequenceEqual(b.ints.Select(k => k.range))) return false;
            if(a.isActive != b.isActive) return false;
            for(int i = 0; i < a.ints.Length; i++)
            {
                if(!a.ints[i].Item3.SequenceEqual(b.ints[i].Item3)) return false;
            }
            return true;
        }
    }
}
