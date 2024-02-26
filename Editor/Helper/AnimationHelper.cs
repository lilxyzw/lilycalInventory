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
