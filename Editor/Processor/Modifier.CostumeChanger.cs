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

                // 各衣装の設定値とprefab初期値を取得したAnimationClipを作成
                for(int i = 0; i < changer.costumes.Length; i++)
                {
                    var costume = changer.costumes[i];
                    (clipDefaults[i], clipChangeds[i]) = costume.parametersPerMenu.CreateClip(ctx, costume.menuName);
                }

                // 同期事故防止のためにオブジェクトのオンオフ状況をコンポーネントの設定に合わせる
                foreach(var toggler in changer.costumes[0].parametersPerMenu.objects)
                {
                    if(toggler.obj) toggler.obj.SetActive(toggler.value);
                }

                // prefab初期値AnimationClipをマージ
                var clipDefault = InternalClip.MergeAndCreate(clipDefaults);

                // 各衣装の未設定値をprefab初期値で埋める
                var clips = new AnimationClip[clipChangeds.Length];
                for(int i = 0; i < clipChangeds.Length; i++)
                {
                    clipChangeds[i] = InternalClip.MergeAndCreate(clipChangeds[i], clipDefault);
                    clipChangeds[i].name = $"{changer.costumes[i].menuName}_Merged";
                    clips[i] = clipChangeds[i].ToClip();
                    AssetDatabase.AddObjectToAsset(clips[i], ctx.AssetContainer);
                }

                // AnimatorControllerに追加
                if(root) AnimationHelper.AddCostumeChangerTree(controller, clips, name, root);
                else AnimationHelper.AddCostumeChangerLayer(controller, hasWriteDefaultsState, clips, name);

                #if LIL_VRCSDK3A
                // パラメーターを追加
                parameters.AddParameterInt(name, changer.isLocalOnly, changer.isSave);
                #endif
            }
        }
    }
}
