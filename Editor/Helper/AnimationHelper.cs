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
                path = renderer.GetPathInAvatarFast(),
                propertyName = $"blendShape.{blendShape}",
                type = renderer.GetType()
            };
        }

        internal static EditorCurveBinding CreateToggleBinding(GameObject gameObject)
        {
            return new EditorCurveBinding
            {
                path = gameObject.GetPathInAvatarFast(),
                propertyName = "m_IsActive",
                type = gameObject.GetType()
            };
        }

        internal static EditorCurveBinding CreateMaterialReplaceBinding(Renderer renderer, int slot)
        {
            return new EditorCurveBinding
            {
                path = renderer.GetPathInAvatarFast(),
                propertyName = $"m_Materials.Array.data[{slot}]",
                type = renderer.GetType()
            };
        }

        internal static EditorCurveBinding CreateMaterialPropertyBinding(Renderer renderer, string name)
        {
            return new EditorCurveBinding
            {
                path = renderer.GetPathInAvatarFast(),
                propertyName = $"material.{name}",
                type = renderer.GetType()
            };
        }

        // AnimatorControllerでWriteDefaultsが使われているか
        // 使われている場合はlilycalInventoryの方でもWriteDefaultsをオンにします
        internal static bool HasWriteDefaultsState(this AnimatorController controller)
        {
            foreach(var layer in controller.layers)
                if(layer.stateMachine.HasWriteDefaultsState()) return true;
            return false;
        }

        private static bool HasWriteDefaultsState(this AnimatorStateMachine stateMachine)
        {
            if(stateMachine.states.Length == 1 && stateMachine.states[0].state.motion is BlendTree b && b.blendType == BlendTreeType.Direct)
                return false;
            foreach(var state in stateMachine.states)
                if(state.state.writeDefaultValues) return true;
            foreach(var childStateMachine in stateMachine.stateMachines)
                if(childStateMachine.stateMachine.HasWriteDefaultsState()) return true;
            return false;
        }

        internal static bool TryAddParameter(this AnimatorController controller, string name, bool defaultValue)
        {
            if(controller.parameters.Any(p => p.name == name)) return false;
            controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Bool, defaultBool = defaultValue });
            return true;
        }

        internal static bool TryAddParameter(this AnimatorController controller, string name, int defaultValue)
        {
            if(controller.parameters.Any(p => p.name == name)) return false;
            controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Int, defaultInt = defaultValue });
            return true;
        }

        internal static bool TryAddParameter(this AnimatorController controller, string name, float defaultValue)
        {
            if(controller.parameters.Any(p => p.name == name)) return false;
            controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });
            return true;
        }
    }
}
