using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace jp.lilxyzw.avatarmodifier
{
    using runtime;

    internal partial class Modifier
    {
        internal static void ApplySmoothChanger(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, SmoothChanger[] changers
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu, VRCExpressionParameters parameters, Dictionary<MenuFolder, VRCExpressionsMenu> dic
        #endif
        )
        {
            foreach(var changer in changers)
            {
                if(changer.frames.Length == 0) continue;
                var name = changer.menuName;
                var clipDefaults = new AnimationClip[changer.frames.Length];
                var clipChangeds = new AnimationClip[changer.frames.Length];
                var frames = new float[changer.frames.Length];
                for(int i = 0; i < changer.frames.Length; i++)
                {
                    var frame = changer.frames[i];
                    var frameValue = Mathf.Clamp01(frame.frameValue);
                    var clip2 = frame.parametersPerMenu.CreateClip(ctx, $"{name}_{i}");
                    clipDefaults[i] = clip2.Item1;
                    clipChangeds[i] = clip2.Item2;
                    frames[i] = frameValue;
                }
                var clipDefault = AnimationHelper.MergeClips(clipDefaults);
                for(int i = 0; i < clipChangeds.Length; i++)
                {
                    clipChangeds[i] = AnimationHelper.MergeAndCreate(clipChangeds[i], clipDefault);
                    clipChangeds[i].name = $"{name}_{i}_Merged";
                    AssetDatabase.AddObjectToAsset(clipChangeds[i], ctx.AssetContainer);
                }
                AddSmoothChangerLayer(controller, hasWriteDefaultsState, clipChangeds, frames, name, changer);

                #if LIL_VRCSDK3A
                var parentMenu = menu;
                var parent = changer.GetMenuParent();
                if(parent && dic.ContainsKey(parent)) parentMenu = dic[parent];
                parentMenu.controls.Add(changer.GetMenuControlRadialPuppet());
                parameters.AddParameterFloat(name, changer.isLocalOnly, changer.isSave, changer.defaultFrameValue);
                #endif
            }
        }

        private static void AddSmoothChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, float[] frames, string name, SmoothChanger changer)
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
            #if LIL_VRCSDK3A
            var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.localOnly = changer.isLocalOnly;
            var parameter = new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Set,
                name = name,
                value = changer.defaultFrameValue
            };
            driver.parameters.Add(parameter);
            #endif
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
    }
}
