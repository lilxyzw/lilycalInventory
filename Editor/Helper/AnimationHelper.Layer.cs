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
        internal static void AddItemTogglerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name, string parameterName, bool defaultValue)
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

            // ItemTogglerにはアバターアップロード/リセット時の状態からメニューを操作したときにどういう状態にするかが設定されているため、
            // パラメーターのデフォルト値がfalseならパラメーターがtrueのとき、
            // パラメーターのデフォルト値がtrueならパラメーターがfalseのとき、
            // ItemTogglerに設定されている状態にする必要がある
            // ゆえに、
            // defaultValueがfalseの場合、ItemTogglerに設定されている状態にする条件はAnimatorConditionMode.Ifとなり、
            // defaultValueがtrueの場合、ItemTogglerに設定されている状態にする条件はAnimatorConditionMode.IfNotとなる
            var transitionToChanged = stateDefault.AddTransition(stateChanged);
            transitionToChanged.AddCondition(!defaultValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, parameterName);
            transitionToChanged.duration = 0;
            var transitionToDefault = stateChanged.AddTransition(stateDefault);
            transitionToDefault.AddCondition(!defaultValue ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0, parameterName);
            transitionToDefault.duration = 0;

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
            controller.TryAddParameter(parameterName, defaultValue);
        }

        internal static void AddCostumeChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, string name, string parameterName, int defaultValue)
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
                if(i == defaultValue) stateMachine.defaultState = state;

                stateMachine.AddEntryTransition(state).AddCondition(AnimatorConditionMode.Equals, i, parameterName);
                var toExit = state.AddExitTransition();
                toExit.AddCondition(AnimatorConditionMode.NotEqual, i, parameterName);
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
            controller.TryAddParameter(parameterName, defaultValue);
        }

        internal static void AddSmoothChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, float[] frames, string name, string parameterName, float defaultValue)
        {
            var stateMachine = new AnimatorStateMachine();
            var tree = new BlendTree
            {
                blendParameter = parameterName,
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
            controller.TryAddParameter(parameterName, defaultValue);
        }

        // 複数コンポーネントから操作されるオブジェクト用
        internal static void AddMultiConditionLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name, (string name, bool toActive, bool defaultValue)[] bools, (string name, bool[] toActives, int defaultValue)[] ints, bool isActive)
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

            AddConditions(controller, stateDefault, stateChanged, bools, ints, isActive);

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
        }

        private static void AddConditions(AnimatorController controller, AnimatorState stateDefault, AnimatorState stateChanged, (string name, bool toActive, bool defaultValue)[] bools, (string name, bool[] toActives, int defaultValue)[] ints, bool isActive)
        {
            var stateActive = isActive ? stateDefault : stateChanged;
            var stateInactive = isActive ? stateChanged : stateDefault;

            var transitionToActive = stateInactive.AddTransition(stateActive);
            transitionToActive.duration = 0;

            // 非アクティブにする条件をor、アクティブにする条件をandにする
            // https://github.com/lilxyzw/lilycalInventory/pull/70#issuecomment-2107029075
            foreach(var (name, toActive, defaultValue) in bools)
            {
                // ItemTogglerにはアバターアップロード/リセット時の状態からメニューを操作したときにどういう状態にするかが設定されているため、
                // パラメーターのデフォルト値がfalseならパラメーターがtrueのとき、
                // パラメーターのデフォルト値がtrueならパラメーターがfalseのとき、
                // ItemTogglerに設定されている状態にする必要がある
                // よって、
                // ItemTogglerに非アクティブが設定されていてパラメーターのデフォルト値がfalseであるか、
                // ItemTogglerにアクティブが設定されていてパラメーターのデフォルト値がtrueである場合、
                // 非アクティブにするのはパラメーターがtrueのときであり、
                // ItemTogglerに非アクティブが設定されていてパラメーターのデフォルト値がtrueであるか、
                // ItemTogglerにアクティブが設定されていてパラメーターのデフォルト値がfalseである場合、
                // 非アクティブにするのはパラメーターがfalseのときである
                // ゆえに、
                // toActiveとdefaultValueが等しい場合、非アクティブにする条件はAnimatorConditionMode.Ifとなり、
                // toActiveとdefaultValueが異なる場合、非アクティブにする条件はAnimatorConditionMode.IfNotとなる
                var transitionToInactive = stateActive.AddTransition(stateInactive);
                transitionToInactive.duration = 0;
                transitionToInactive.AddCondition(toActive == defaultValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, name);

                transitionToActive.AddCondition(toActive == defaultValue ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0, name);

                controller.TryAddParameter(name, defaultValue);
            }

            // 非アクティブにする条件をor、アクティブにする条件をandにする
            // https://github.com/lilxyzw/lilycalInventory/pull/70#issuecomment-2107029075
            foreach(var (name, toActives, defaultValue) in ints)
            {
                // アクティブにする値を使うと両方の遷移でandとorの組み合わせを考えることになるが、
                // 非アクティブにする値を使うと一方の遷移はorだけ、もう一方の遷移はandだけ考えればよくなる
                foreach(var value in Enumerable.Range(0, toActives.Length).Where(i => !toActives[i]))
                {
                    var transitionToInactive = stateActive.AddTransition(stateInactive);
                    transitionToInactive.duration = 0;
                    transitionToInactive.AddCondition(AnimatorConditionMode.Equals, value, name);

                    transitionToActive.AddCondition(AnimatorConditionMode.NotEqual, value, name);
                }

                controller.TryAddParameter(name, defaultValue);
            }
        }
    }
}
