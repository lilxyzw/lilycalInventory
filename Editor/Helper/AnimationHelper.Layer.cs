using System.Linq;
using jp.lilxyzw.lilycalinventory.runtime;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    // Layerで処理
    // AnimationHelper.DirectBlendTree.cs と対になっています
    internal static partial class AnimationHelper
    {
        internal static void AddItemTogglerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name)
        {
            // オンオフアニメーションを追加
            var stateDefault = new AnimatorState
            {
                motion = clipDefault,
                name = clipDefault.name,
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateChanged = new AnimatorState
            {
                motion = clipChanged,
                name = clipChanged.name,
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateMachine = new AnimatorStateMachine();
            stateMachine.AddState(stateDefault, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.AddState(stateChanged, stateMachine.entryPosition + new Vector3(450,0,0));
            stateMachine.defaultState = stateDefault;

            var transitionToChanged = stateDefault.AddTransition(stateChanged);
            transitionToChanged.AddCondition(AnimatorConditionMode.If, 0, name);
            transitionToChanged.duration = 0;
            var transitionToDefault = stateChanged.AddTransition(stateDefault);
            transitionToDefault.AddCondition(AnimatorConditionMode.IfNot, 0, name);
            transitionToDefault.duration = 0;

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Bool);
        }

        internal static void AddCostumeChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, string name)
        {
            var stateMachine = new AnimatorStateMachine();

            // 衣装の数だけアニメーションを追加
            for(int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                var state = new AnimatorState
                {
                    motion = clip,
                    name = clip.name,
                    writeDefaultValues = hasWriteDefaultsState
                };
                stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(200,clips.Length*25-i*50,0));
                stateMachine.AddEntryTransition(state).AddCondition(AnimatorConditionMode.Equals, i, name);
                var toExit = state.AddExitTransition();
                toExit.AddCondition(AnimatorConditionMode.NotEqual, i, name);
                toExit.duration = 0;
            }

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Int);
        }

        internal static void AddSmoothChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, float[] frames, string name, SmoothChanger changer)
        {
            var stateMachine = new AnimatorStateMachine();
            var tree = new BlendTree
            {
                blendParameter = name,
                blendType = BlendTreeType.Simple1D,
                name = name,
                useAutomaticThresholds = false
            };

            // フレームの数だけアニメーションを追加
            for(int i = 0; i < clips.Length; i++)
                tree.AddChild(clips[i], frames[i]);

            var state = new AnimatorState
            {
                motion = tree,
                name = tree.name,
                writeDefaultValues = hasWriteDefaultsState
            };
            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.AddEntryTransition(state);

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Float);
        }

        // 複数コンポーネントから操作されるオブジェクト用
        internal static void AddMultiConditionLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name, (string name, bool isChange)[] bools, (string name, bool[] isChanges)[] ints)
        {
            var stateDefault = new AnimatorState
            {
                motion = clipDefault,
                name = clipDefault.name,
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateChanged = new AnimatorState
            {
                motion = clipChanged,
                name = clipChanged.name,
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateMachine = new AnimatorStateMachine();

            stateMachine.AddState(stateDefault, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.AddState(stateChanged, stateMachine.entryPosition + new Vector3(450,0,0));
            stateMachine.defaultState = stateDefault;

            AddConditions(controller, stateDefault, stateChanged, bools, ints);

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
        }

        private static void AddConditions(AnimatorController controller, AnimatorState stateDefault, AnimatorState stateChanged, (string name, bool isChange)[] bools, (string name, bool[] isChanges)[] ints)
        {
            var transitionToChanged = stateDefault.AddTransition(stateChanged);
            transitionToChanged.duration = 0;

            // デフォルトに戻す条件をor、デフォルトから変更する条件をandにする
            foreach(var (name, isChange) in bools)
            {
                var transitionToDefault = stateChanged.AddTransition(stateDefault);
                transitionToDefault.duration = 0;
                transitionToDefault.AddCondition(!isChange ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, name);

                transitionToChanged.AddCondition(!isChange ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0, name);

                if(!controller.parameters.Any(p => p.name == name))
                    controller.AddParameter(name, AnimatorControllerParameterType.Bool);
            }

            // デフォルトに戻す条件をor、デフォルトから変更する条件をandにする
            foreach(var (name, isChanges) in ints)
            {
                // デフォルトから変更する値を使うと両方の遷移でandとorの組み合わせを考えることになるが、
                // デフォルトに戻す値を使うと一方の遷移はorだけ、もう一方の遷移はandだけ考えればよくなる
                foreach(var value in Enumerable.Range(0, isChanges.Length).Where(i => !isChanges[i]))
                {
                    var transitionToDefault = stateChanged.AddTransition(stateDefault);
                    transitionToDefault.duration = 0;
                    transitionToDefault.AddCondition(AnimatorConditionMode.Equals, value, name);

                    transitionToChanged.AddCondition(AnimatorConditionMode.NotEqual, value, name);
                }

                if(!controller.parameters.Any(p => p.name == name))
                    controller.AddParameter(name, AnimatorControllerParameterType.Int);
            }
        }
    }
}
