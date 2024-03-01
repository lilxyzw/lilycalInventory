using System.Collections.Generic;
using System.Linq;
using jp.lilxyzw.lilycalinventory.runtime;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class AnimationHelper
    {
        internal static void CreateLayer(AnimatorController controller, out BlendTree root)
        {
            var parameterName = "Weight";
            if(controller.parameters.Any(p => p.name == parameterName))
            {
                for(int i = 0; i < 100; i++)
                {
                    var nameTemp = $"{parameterName}_{i}";
                    if(controller.parameters.Any(p => p.name == nameTemp)) continue;
                    parameterName = nameTemp;
                    break;
                }
            }
            controller.AddParameter(parameterName, AnimatorControllerParameterType.Float);
            var parameters = controller.parameters;
            parameters[controller.parameters.Length-1].defaultFloat = 1;
            controller.parameters = parameters;

            // Root
            root = new BlendTree
            {
                blendType = BlendTreeType.Direct,
                blendParameter = parameterName,
                name = "Root",
                useAutomaticThresholds = false
            };

            var state = new AnimatorState
            {
                motion = root,
                name = "Root",
                writeDefaultValues = true
            };

            var stateMachine = new AnimatorStateMachine();

            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.defaultState = state;

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = ConstantValues.TOOL_NAME,
                stateMachine = stateMachine
            };
            controller.AddLayer(layer);
        }

        internal static void SetParameter(BlendTree root)
        {
            var children = root.children;
            for(int i = 0; i < children.Length; i++)
                children[i].directBlendParameter = root.blendParameter;
            root.children = children;
        }

        internal static void AddSimpleTree(AnimatorController controller, AnimationClip clipDefault, AnimationClip clipChanged, string name, BlendTree root)
        {
            var layer = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                blendParameter = name,
                name = name,
                useAutomaticThresholds = true
            };
            layer.AddChild(clipDefault);
            layer.AddChild(clipChanged);

            root.AddChild(layer);
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Float);
        }

        internal static void AddCostumeChangerTree(AnimatorController controller, AnimationClip[] clips, string name, BlendTree root)
        {
            var layer = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                blendParameter = name,
                name = name,
                useAutomaticThresholds = false
            };
            for(int i = 0; i < clips.Length; i++)
                layer.AddChild(clips[i], i);

            root.AddChild(layer);
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Float);
        }

        internal static void AddSmoothChangerTree(AnimatorController controller, AnimationClip[] clips, float[] frames, string name, BlendTree root)
        {
            var layer = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                blendParameter = name,
                name = name,
                useAutomaticThresholds = false
            };
            for(int i = 0; i < clips.Length; i++)
                layer.AddChild(clips[i], frames[i]);

            root.AddChild(layer);
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Float);
        }

        internal static void AddMultiConditionTree(AnimatorController controller, AnimationClip clipDefault, AnimationClip clipChanged, string[] bools, (string,int,(int,bool)[])[] ints, BlendTree root, bool isActive)
        {
            BlendTree parent = root;
            BlendTree layer = null;
            for(int i = 0; i < bools.Length; i++)
            {
                layer = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = bools[i],
                    name = bools[i],
                    useAutomaticThresholds = true
                };
                if(parent != root)
                {
                    if(isActive)
                    {
                        parent.AddChild(layer);
                        parent.AddChild(clipChanged);
                    }
                    else
                    {
                        if(parent != root) parent.AddChild(clipDefault);
                        parent.AddChild(layer);
                    }
                }
                else
                {
                    parent.AddChild(layer);
                }
                if(!controller.parameters.Any(p => p.name == bools[i]))
                    controller.AddParameter(bools[i], AnimatorControllerParameterType.Float);
                parent = layer;
            }
            var boolEnd = parent;
            if(!isActive) boolEnd.AddChild(clipDefault);

            var prev = new HashSet<int>{0};
            int loops = 1;
            for(int i = 0; i < ints.Length; i++)
            {
                var newInts = new HashSet<int>(ints[i].Item3.Where(p => p.Item2).Select(p => p.Item1));
                var inv = ints[i].Item3.Where(p => !p.Item2).Select(p => p.Item1);
                if(inv.Count() > 0) newInts.UnionWith(Enumerable.Range(0,ints[i].Item2).Except(inv));
                layer = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = ints[i].Item1,
                    name = ints[i].Item1,
                    useAutomaticThresholds = false
                };

                if(i > 0) loops = ints[i-1].Item2;
                if(parent != root)
                {
                    for(int j = 0; j < loops; j++)
                    {
                        if(prev.Contains(j)) parent.AddChild(layer, j);
                        else parent.AddChild(isActive ? clipChanged : clipDefault, j);
                    }
                }
                else
                {
                    parent.AddChild(layer);
                }
                prev = newInts;

                if(!controller.parameters.Any(p => p.name == ints[i].Item1))
                    controller.AddParameter(ints[i].Item1, AnimatorControllerParameterType.Float);
                parent = layer;
            }
            for(int j = 0; j < ints[ints.Length-1].Item2; j++)
            {
                if(prev.Contains(j)) parent.AddChild(isActive ? clipDefault : clipChanged, j);
                else parent.AddChild(isActive ? clipChanged : clipDefault, j);
            }

            if(isActive) boolEnd.AddChild(clipChanged);
        }
    }
}
