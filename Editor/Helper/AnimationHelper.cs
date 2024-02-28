using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class AnimationHelper
    {
        internal static EditorCurveBinding CreateBlendShapeBinding(SkinnedMeshRenderer renderer, string blendShape)
        {
            return new EditorCurveBinding
            {
                path = renderer.GetPathInAvatar(),
                propertyName = $"blendShape.{blendShape}",
                type = typeof(SkinnedMeshRenderer)
            };
        }

        internal static EditorCurveBinding CreateToggleBinding(GameObject gameObject)
        {
            return new EditorCurveBinding
            {
                path = gameObject.GetPathInAvatar(),
                propertyName = "m_IsActive",
                type = typeof(GameObject)
            };
        }

        internal static EditorCurveBinding CreateMaterialReplaceBinding(Renderer renderer, int slot)
        {
            return new EditorCurveBinding
            {
                path = renderer.GetPathInAvatar(),
                propertyName = $"m_Materials.Array.data[{slot}]",
                type = typeof(Renderer)
            };
        }

        internal static EditorCurveBinding CreateMaterialPropertyBinding(Renderer renderer, string name)
        {
            return new EditorCurveBinding
            {
                path = renderer.GetPathInAvatar(),
                propertyName = $"material.{name}",
                type = typeof(Renderer)
            };
        }

        private static AnimationCurve SimpleCurve(float value)
        {
            var curve = new AnimationCurve();
            curve.AddKey(0, value);
            return curve;
        }

        private static AnimationCurve SimpleCurve(bool value) => SimpleCurve(value ? 1 : 0);

        private static ObjectReferenceKeyframe[] SimpleCurve(Object obj)
        {
            return new[]{new ObjectReferenceKeyframe
            {
                time = 0,
                value = obj
            }};
        }

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
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Bool);
        }

        internal static void AddMultiConditionLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip clipDefault, AnimationClip clipChanged, string name, string[] bools, (string,(int,bool)[])[] ints, bool isActive)
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

        private static void AddConditions(AnimatorState stateDefault, AnimatorState stateChanged, string[] bools, (string,(int,bool)[])[] ints, bool isActive)
        {

            var toChangeds = ints.Select(i => i.Item2.Length).Aggregate((a, b) => a * b);
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
                for(int i = 0; i < b.Item2.Length; i++)
                    transitionToChangeds[i*offset].AddCondition(!b.Item2[i].Item2 ? AnimatorConditionMode.NotEqual : AnimatorConditionMode.Equals, b.Item2[i].Item1, b.Item1);

                offset *= b.Item2.Length;

                var transitionToDefault = stateChanged.AddTransition(stateDefault);
                transitionToDefault.duration = 0;
                foreach(var c in b.Item2)
                    transitionToDefault.AddCondition(!c.Item2 ? AnimatorConditionMode.Equals : AnimatorConditionMode.NotEqual, c.Item1, b.Item1);
            }
        }

        internal static void Merge(this AnimationClip clip, AnimationClip clip2)
        {
            foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip2))
            {
                AnimationUtility.SetObjectReferenceCurve(clip, binding, AnimationUtility.GetObjectReferenceCurve(clip2, binding));
            }
            foreach(var binding in AnimationUtility.GetCurveBindings(clip2))
            {
                AnimationUtility.SetEditorCurve(clip, binding, AnimationUtility.GetEditorCurve(clip2, binding));
            }
        }

        internal static AnimationClip MergeAndCreate(AnimationClip clip, AnimationClip clip2)
        {
            var clipNew = new AnimationClip{name = $"{clip.name}Merged"};
            clipNew.Merge(clip2);
            clipNew.Merge(clip);
            return clipNew;
        }

        internal static AnimationClip MergeClips(AnimationClip[] clips)
        {
            var clipNew = new AnimationClip{name = $"{clips[0].name}Merged"};
            foreach(var clip in clips)
                clipNew.Merge(clip);
            return clipNew;
        }

        internal static bool HasWriteDefaultsState(this AnimatorController controller)
        {
            foreach(var layer in controller.layers)
            {
                if(layer.stateMachine.HasWriteDefaultsState()) return true;
            }
            return false;
        }

        private static bool HasWriteDefaultsState(this AnimatorStateMachine stateMachine)
        {
            foreach(var state in stateMachine.states)
            {
                if(state.state.writeDefaultValues) return true;
            }
            foreach(var childStateMachine in stateMachine.stateMachines)
            {
                if(childStateMachine.stateMachine.HasWriteDefaultsState()) return true;
            }
            return false;
        }
    }
}
