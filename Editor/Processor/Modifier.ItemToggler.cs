using UnityEditor;
using UnityEditor.Animations;

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
        internal static void ApplyItemToggler(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers, BlendTree root
        #if LIL_VRCSDK3A
        , VRCExpressionParameters parameters
        #endif
        )
        {
            foreach(var toggler in togglers)
            {
                var name = toggler.menuName;
                if(toggler.parameter.objects.Length + toggler.parameter.blendShapeModifiers.Length + toggler.parameter.materialReplacers.Length + toggler.parameter.materialPropertyModifiers.Length > 0)
                {
                    // コンポーネントの設定値とprefab初期値を取得したAnimationClipを作成
                    var clips = toggler.parameter.CreateClip(ctx, name);
                    var (clipDefault, clipChanged) = (clips.clipDefault.ToClip(), clips.clipChanged.ToClip());
                    AssetDatabase.AddObjectToAsset(clipDefault, ctx.AssetContainer);
                    AssetDatabase.AddObjectToAsset(clipChanged, ctx.AssetContainer);

                    // AnimatorControllerに追加
                    if(root) AnimationHelper.AddItemTogglerTree(controller, clipDefault, clipChanged, name, toggler.flipToggle, root);
                    else AnimationHelper.AddItemTogglerLayer(controller, hasWriteDefaultsState, clipDefault, clipChanged, name, toggler.flipToggle);
                }

                #if LIL_VRCSDK3A
                // パラメーターを追加
                parameters.AddParameterToggle(name, toggler.isLocalOnly, toggler.isSave, toggler.flipToggle);
                #endif
            }
        }
    }
}
