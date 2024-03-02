using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal partial class Modifier
    {
        internal static void ApplySmoothChanger(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, SmoothChanger[] changers, BlendTree root
        #if LIL_VRCSDK3A
        , VRCExpressionParameters parameters
        #endif
        )
        {
            foreach(var changer in changers)
            {
                if(changer.frames.Length == 0) continue;

                var name = changer.menuName;
                var clipDefaults = new InternalClip[changer.frames.Length];
                var clipChangeds = new InternalClip[changer.frames.Length];
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

                var clipDefault = InternalClip.MergeAndCreate(clipDefaults);
                var clips = new AnimationClip[clipChangeds.Length];
                for(int i = 0; i < clipChangeds.Length; i++)
                {
                    clipChangeds[i] = InternalClip.MergeAndCreate(clipChangeds[i], clipDefault);
                    clipChangeds[i].name = $"{name}_{i}_Merged";
                    clips[i] = clipChangeds[i].ToClip();
                    AssetDatabase.AddObjectToAsset(clips[i], ctx.AssetContainer);
                }
                if(root) AnimationHelper.AddSmoothChangerTree(controller, clips, frames, name, root);
                else AnimationHelper.AddSmoothChangerLayer(controller, hasWriteDefaultsState, clips, frames, name, changer);

                #if LIL_VRCSDK3A
                parameters.AddParameterFloat(name, changer.isLocalOnly, changer.isSave, changer.defaultFrameValue);
                #endif
            }
        }
    }
}
