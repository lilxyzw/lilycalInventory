using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ApplyPreset(AnimatorController controller, bool hasWriteDefaultsState, Preset[] presets, List<InternalParameter> parameters)
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
                    var stateChanged = new AnimatorState
                    {
                        motion = emptyClip,
                        name = preset.menuName,
                        writeDefaultValues = hasWriteDefaultsState
                    };
                    stateMachine.AddState(stateChanged, stateMachine.entryPosition + new Vector3(450,100*(i-presets.Length/2),0));

                    var transitionToChanged = stateDefault.AddTransition(stateChanged);
                    transitionToChanged.AddCondition(AnimatorConditionMode.If, 1, preset.parameterName);
                    transitionToChanged.duration = 0;
                    var transitionToDefault = stateChanged.AddTransition(stateDefault);
                    transitionToDefault.AddCondition(AnimatorConditionMode.IfNot, 1, preset.parameterName);
                    transitionToDefault.duration = 0;

                    var driver = stateChanged.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                    driver.localOnly = true;
                    foreach(var item in preset.presetItems)
                    {
                        if(!item.obj) continue;
                        string parameterName;
                        switch(item.obj)
                        {
                            case ItemToggler c: parameterName = c.parameterName; break;
                            case CostumeChanger c: parameterName = c.isLocalOnly ? c.parameterName : c.parameterNameLocal; break;
                            case SmoothChanger c: parameterName = c.parameterName; break;
                            default: throw new System.Exception($"Unknown type {item.obj}");
                        }

                        driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter(){
                            type = VRC_AvatarParameterDriver.ChangeType.Set,
                            name = parameterName,
                            value = item.value
                        });
                    }
                    AssetDatabase.AddObjectToAsset(driver, ctx.AssetContainer);

                    controller.TryAddParameter(preset.parameterName, false);

                    // パラメーターを追加
                    parameters.Add(new InternalParameter(preset.parameterName, 0, true, false, InternalParameterType.Bool));

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
}
