using System.Collections.Generic;
using System.Linq;
using jp.lilxyzw.lilycalinventory.runtime;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class AnimationHelper
    {

        internal static void AddSimpleLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name)
        {
            var stateDefault = new AnimatorState
            {
                motion = clipDefault,
                name = "Off",
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateChanged = new AnimatorState
            {
                motion = clipChanged,
                name = "On",
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
            //if(!controller.parameters.Any(p => p.name == name))
            //    controller.AddParameter(name, AnimatorControllerParameterType.Bool);
        }

        internal static void AddCostumeChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, string name)
        {
            var stateMachine = new AnimatorStateMachine();

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

        internal static void AddMultiConditionLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name, string[] bools, (string,int,(int,bool)[])[] ints, bool isActive)
        {
            var stateDefault = new AnimatorState
            {
                motion = clipDefault,
                name = isActive ? "On" : "Off",
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateChanged = new AnimatorState
            {
                motion = clipChanged,
                name = isActive ? "Off" : "On",
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateMachine = new AnimatorStateMachine();

            stateMachine.AddState(stateDefault, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.AddState(stateChanged, stateMachine.entryPosition + new Vector3(450,0,0));
            stateMachine.defaultState = stateDefault;

            if(!isActive) AddConditions(stateDefault, stateChanged, bools, ints, isActive);
            else AddConditions(stateChanged, stateDefault, bools, ints, isActive);

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
        }

        private static void AddConditions(AnimatorState stateDefault, AnimatorState stateChanged, string[] bools, (string,int,(int,bool)[])[] ints, bool isActive)
        {

            var toChangeds = ints.Select(i => i.Item3.Length).Aggregate((a, b) => a * b);
            var transitionToChangeds = new AnimatorStateTransition[toChangeds];

            for(int i = 0; i < toChangeds; i++)
            {
                transitionToChangeds[i] = stateDefault.AddTransition(stateChanged);
                transitionToChangeds[i].duration = 0;
            }

            foreach(var b in bools)
            {
                foreach(var transitionToChanged in transitionToChangeds)
                    transitionToChanged.AddCondition(isActive ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0, b);

                var transitionToDefault = stateChanged.AddTransition(stateDefault);
                transitionToDefault.duration = 0;
                transitionToDefault.AddCondition(isActive ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, b);
            }

            int offset = 1;
            foreach(var b in ints)
            {
                for(int i = 0; i < b.Item3.Length; i++)
                    transitionToChangeds[i*offset].AddCondition(!b.Item3[i].Item2 ? AnimatorConditionMode.NotEqual : AnimatorConditionMode.Equals, b.Item3[i].Item1, b.Item1);

                offset *= b.Item3.Length;

                var transitionToDefault = stateChanged.AddTransition(stateDefault);
                transitionToDefault.duration = 0;
                foreach(var c in b.Item3)
                    transitionToDefault.AddCondition(!c.Item2 ? AnimatorConditionMode.Equals : AnimatorConditionMode.NotEqual, c.Item1, b.Item1);
            }
        }
    }
}
