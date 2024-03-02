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
        internal static void ApplyCostumeChanger(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, CostumeChanger[] changers, BlendTree root
        #if LIL_VRCSDK3A
        , VRCExpressionParameters parameters
        #endif
        )
        {
            foreach(var changer in changers)
            {
                if(changer.costumes.Length == 0) continue;
                var name = changer.menuName;
                var clipDefaults = new InternalClip[changer.costumes.Length];
                var clipChangeds = new InternalClip[changer.costumes.Length];
                for(int i = 0; i < changer.costumes.Length; i++)
                {
                    var costume = changer.costumes[i];
                    var clip2 = costume.parametersPerMenu.CreateClip(ctx, costume.menuName);
                    clipDefaults[i] = clip2.Item1;
                    clipChangeds[i] = clip2.Item2;
                }
                foreach(var toggler in changer.costumes[0].parametersPerMenu.objects)
                {
                    if(toggler.obj) toggler.obj.SetActive(toggler.value);
                }
                var clipDefault = InternalClip.MergeAndCreate(clipDefaults);
                var clips = new AnimationClip[clipChangeds.Length];
                for(int i = 0; i < clipChangeds.Length; i++)
                {
                    clipChangeds[i] = InternalClip.MergeAndCreate(clipChangeds[i], clipDefault);
                    clipChangeds[i].name = $"{changer.costumes[i].menuName}_Merged";
                    clips[i] = clipChangeds[i].ToClip();
                    AssetDatabase.AddObjectToAsset(clips[i], ctx.AssetContainer);
                }
                if(root) AnimationHelper.AddCostumeChangerTree(controller, clips, name, root);
                else AnimationHelper.AddCostumeChangerLayer(controller, hasWriteDefaultsState, clips, name);

                #if LIL_VRCSDK3A
                parameters.AddParameterInt(name, changer.isLocalOnly, changer.isSave);
                #endif
            }
        }
    }
}
