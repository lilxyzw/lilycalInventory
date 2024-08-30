using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal partial class Modifier
    {
        internal static void ApplyPreset(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, Preset[] presets
        #if LIL_VRCSDK3A
        , VRCExpressionParameters parameters
        #endif
        )
        {
            #if LIL_VRCSDK3A
            if(presets.Length == 0) return;
            var emptyClip = new AnimationClip(){name = "Empty"};
            AssetDatabase.AddObjectToAsset(emptyClip, ctx.AssetContainer);
            var stateDefault = new AnimatorState
            {
                motion = emptyClip,
                name = "Idle",
                writeDefaultValues = hasWriteDefaultsState
            };

            var stateMachine = new AnimatorStateMachine();
            stateMachine.AddState(stateDefault, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.defaultState = stateDefault;

            int i = 0;
            foreach(var preset in presets)
            {
                var name = preset.menuName;
                var stateChanged = new AnimatorState
                {
                    motion = emptyClip,
                    name = name,
                    writeDefaultValues = hasWriteDefaultsState
                };
                stateMachine.AddState(stateChanged, stateMachine.entryPosition + new Vector3(450,100*(i-presets.Length/2),0));

                var transitionToChanged = stateDefault.AddTransition(stateChanged);
                transitionToChanged.AddCondition(AnimatorConditionMode.If, 1, name);
                transitionToChanged.duration = 0;
                var transitionToDefault = stateChanged.AddTransition(stateDefault);
                transitionToDefault.AddCondition(AnimatorConditionMode.IfNot, 1, name);
                transitionToDefault.duration = 0;

                var driver = stateChanged.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                driver.localOnly = true;
                foreach(var item in preset.presetItems)
                {
                    if(item.obj)
                    driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter(){
                        type = VRC_AvatarParameterDriver.ChangeType.Set,
                        name = item.obj.GetMenuName(),
                        value = item.value
                    });
                }
                AssetDatabase.AddObjectToAsset(driver, ctx.AssetContainer);

                controller.TryAddParameter(name, false);

                // パラメーターを追加
                parameters.AddParameterToggle(name, true, false, false);

                i++;
            }

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = "LI Preset",
                stateMachine = stateMachine
            };
            controller.AddLayer(layer);
            #endif
        }
    }
}
