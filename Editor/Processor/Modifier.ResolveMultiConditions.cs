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
            // Find multi condition objects
            var toggleBools = new Dictionary<GameObject, HashSet<string>>();
            var toggleInts = new Dictionary<GameObject, Dictionary<string, (int,HashSet<(int,bool)>)>>();
            togglers.GatherConditions(toggleBools);
            costumeChangers.GatherConditions(toggleInts);
            var multiConditionObjects = toggleBools.Select(b => b.Key).Concat(toggleInts.Select(i => i.Key)).GroupBy(o => o.name).Where(g => g.Count() > 1).Select(g => g.ElementAt(0)).ToArray();

            foreach(var t in togglers)
                t.parameter.objects = t.parameter.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();
            foreach(var c in costumeChangers)
                foreach(var t in c.costumes)
                    t.parametersPerMenu.objects = t.parametersPerMenu.objects.Where(o => !multiConditionObjects.Contains(o.obj)).ToArray();

            // Combine by conditions
            // bools,ints,isActive
            var conditionAndObjects = new Dictionary<(string[],(string,int,(int,bool)[])[],bool), List<GameObject>>();
            var conditions = new HashSet<(string[],(string,int,(int,bool)[])[],bool)>();
            foreach(var o in multiConditionObjects)
            {
                var bools = new string[]{};
                if(toggleBools.ContainsKey(o)) bools = toggleBools[o].OrderBy(a => a).ToArray();
                var ints = new (string,int,(int,bool)[])[]{};
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

            // Create animation
            foreach(var c in conditionAndObjects)
            {
                var bools = c.Key.Item1;
                var ints = c.Key.Item2;

                var name = c.Value.ElementAt(0).name;
                var clipOff = new AnimationClip();
                var clipOn = new AnimationClip();
                clipOff.name = $"{name}_Off";
                clipOn.name = $"{name}_On";
                foreach(var o in c.Value)
                {
                    var toggler = new ObjectToggler
                    {
                        obj = o,
                        value = !c.Key.Item3
                    };
                    toggler.ToClipDefault(clipOff);
                    toggler.ToClip(clipOn);
                }

                AssetDatabase.AddObjectToAsset(clipOff, ctx.AssetContainer);
                AssetDatabase.AddObjectToAsset(clipOn, ctx.AssetContainer);
                if(root) AnimationHelper.AddMultiConditionTree(controller, clipOff, clipOn, bools, ints, root, c.Key.Item3);
                else AnimationHelper.AddMultiConditionLayer(controller, hasWriteDefaultsState, clipOff, clipOn, name, bools, ints, c.Key.Item3);
            }
        }

        private static bool IsSameConditions((string[],(string,int,(int, bool)[])[],bool) a, (string[],(string,int,(int,bool)[])[], bool) b)
        {
            if(!a.Item1.SequenceEqual(b.Item1)) return false;
            if(!a.Item2.Select(k => k.Item1).SequenceEqual(b.Item2.Select(k => k.Item1))) return false;
            if(!a.Item2.Select(k => k.Item2).SequenceEqual(b.Item2.Select(k => k.Item2))) return false;
            if(a.Item3 != b.Item3) return false;
            for(int i = 0; i < a.Item2.Length; i++)
            {
                if(!a.Item2[i].Item3.SequenceEqual(b.Item2[i].Item3)) return false;
            }
            return true;
        }
    }
}
