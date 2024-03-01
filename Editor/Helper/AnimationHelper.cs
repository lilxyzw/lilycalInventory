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

        internal static bool HasWriteDefaultsState(this AnimatorController controller)
        {
            foreach(var layer in controller.layers)
                if(layer.stateMachine.HasWriteDefaultsState()) return true;
            return false;
        }

        private static bool HasWriteDefaultsState(this AnimatorStateMachine stateMachine)
        {
            foreach(var state in stateMachine.states)
                if(state.state.writeDefaultValues) return true;
            foreach(var childStateMachine in stateMachine.stateMachines)
                if(childStateMachine.stateMachine.HasWriteDefaultsState()) return true;
            return false;
        }
    }
}
