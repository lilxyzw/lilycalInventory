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
        internal static void AddItemTogglerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name, bool flipState)
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
            transitionToChanged.AddCondition(flipState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0, name);
            transitionToChanged.duration = 0;
            var transitionToDefault = stateChanged.AddTransition(stateDefault);
            transitionToDefault.AddCondition(flipState ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, name);
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
                controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Bool, defaultBool = flipState });
        }

        internal static void AddCostumeChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, string name, int defaultState)
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
                if(i == defaultState) stateMachine.defaultState = state;

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
                controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Int, defaultInt = defaultState });
        }

        internal static void AddSmoothChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, float[] frames, string name, float defaultValue)
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
                controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });
        }

        // 複数コンポーネントから操作されるオブジェクト用
        internal static void AddMultiConditionLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name, (string name, bool isChange, bool flipState)[] bools, (string name, bool[] isChanges, int defaultState)[] ints)
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

        private static void AddConditions(AnimatorController controller, AnimatorState stateDefault, AnimatorState stateChanged, (string name, bool isChange, bool flipState)[] bools, (string name, bool[] isChanges, int defaultState)[] ints)
        {

            var toChangeds = ints.Select(i => i.isChanges.Count(c => c)).Aggregate(1, (a, b) => a * b);
            var transitionToChangeds = new AnimatorStateTransition[toChangeds];

            for(int i = 0; i < toChangeds; i++)
            {
                transitionToChangeds[i] = stateDefault.AddTransition(stateChanged);
                transitionToChangeds[i].duration = 0;
            }

            // Boolはand条件で処理
            foreach(var (name, isChange, flipState) in bools)
            {
                foreach(var transitionToChanged in transitionToChangeds)
                    transitionToChanged.AddCondition(isChange ^ flipState ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, name);

                var transitionToDefault = stateChanged.AddTransition(stateDefault);
                transitionToDefault.duration = 0;
                transitionToDefault.AddCondition(isChange ^ flipState ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0, name);

                if(!controller.parameters.Any(p => p.name == name))
                    controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Bool, defaultBool = flipState });
            }

            // Intもand条件だが、Int内の各数値はor条件
            int offset = 1;
            foreach(var (name, isChanges, defaultState) in ints)
            {
                var valueToChangeds = Enumerable.Range(0, isChanges.Length).Where(i => isChanges[i]).ToArray();
                for(var i = 0; i < transitionToChangeds.Length; i++)
                    transitionToChangeds[i].AddCondition(AnimatorConditionMode.Equals, valueToChangeds[i / offset % valueToChangeds.Length], name);

                offset *= valueToChangeds.Length;

                var transitionToDefault = stateChanged.AddTransition(stateDefault);
                transitionToDefault.duration = 0;
                foreach(var value in valueToChangeds)
                    transitionToDefault.AddCondition(AnimatorConditionMode.NotEqual, value, name);

                if(!controller.parameters.Any(p => p.name == name))
                    controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Int, defaultInt = defaultState });
            }
        }
    }
}
