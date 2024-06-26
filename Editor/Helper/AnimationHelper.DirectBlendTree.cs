using System.Collections.Generic;
using System.Linq;
using jp.lilxyzw.lilycalinventory.runtime;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    // DirectBlendTreeで処理
    // AnimationHelper.Layer.cs と対になっています
    internal static partial class AnimationHelper
    {
        // lilycalInventoryの全DirectBlendTreeの追加先となるレイヤーを作成
        internal static void CreateLayer(AnimatorController controller, out BlendTree root)
        {
            // 常に1に設定されるWeightプロパティを生成
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

            // 各コンポーネントで生成されるBlendTreeの追加先のBlendTree
            root = new BlendTree
            {
                blendType = BlendTreeType.Direct,
                blendParameter = parameterName,
                name = "Root",
                useAutomaticThresholds = false
            };

            // BlendTreeの追加先のState
            var state = new AnimatorState
            {
                motion = root,
                name = "Root",
                writeDefaultValues = true
            };

            // Stateの追加先のStateMachine
            var stateMachine = new AnimatorStateMachine();
            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.defaultState = state;

            // StateMachineの追加先のLayer
            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = ConstantValues.TOOL_NAME,
                stateMachine = stateMachine
            };
            controller.AddLayer(layer);
        }

        // 子のBlendTreeにパラメーターを設定
        internal static void SetParameter(BlendTree root)
        {
            var children = root.children;
            for(int i = 0; i < children.Length; i++)
                children[i].directBlendParameter = root.blendParameter;
            root.children = children;
        }

        internal static void AddItemTogglerTree(AnimatorController controller, AnimationClip clipDefault, AnimationClip clipChanged, string name, BlendTree root)
        {
            var layer = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                blendParameter = name,
                name = name,
                useAutomaticThresholds = true
            };

            // オンオフアニメーションを追加
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

            // 衣装の数だけアニメーションを追加
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

            // フレームの数だけアニメーションを追加
            for(int i = 0; i < clips.Length; i++)
                layer.AddChild(clips[i], frames[i]);

            root.AddChild(layer);

            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Float);
        }

        // 複数コンポーネントから操作されるオブジェクト用
        internal static void AddMultiConditionTree(AnimatorController controller, AnimationClip clipDefault, AnimationClip clipChanged, string[] bools, (string name, int range,(int value, bool isActive)[])[] ints, BlendTree root, bool isActive)
        {
            BlendTree parent = root;
            BlendTree layer = null;

            // Boolはand条件で処理
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

            // Intもand条件だが、Int内の各数値はor条件
            var prev = new HashSet<int>{0};
            int loops = 1;
            for(int i = 0; i < ints.Length; i++)
            {
                var newInts = new HashSet<int>(ints[i].Item3.Where(p => p.isActive).Select(p => p.value));
                var inv = ints[i].Item3.Where(p => !p.isActive).Select(p => p.value);
                if(inv.Count() > 0) newInts.UnionWith(Enumerable.Range(0,ints[i].range).Except(inv));
                layer = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = ints[i].name,
                    name = ints[i].name,
                    useAutomaticThresholds = false
                };

                if(i > 0) loops = ints[i-1].range;
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

                if(!controller.parameters.Any(p => p.name == ints[i].name))
                    controller.AddParameter(ints[i].name, AnimatorControllerParameterType.Float);
                parent = layer;
            }
            for(int j = 0; j < ints[ints.Length-1].range; j++)
            {
                if(prev.Contains(j)) parent.AddChild(isActive ? clipDefault : clipChanged, j);
                else parent.AddChild(isActive ? clipChanged : clipDefault, j);
            }

            if(isActive) boolEnd.AddChild(clipChanged);
        }
    }
}
