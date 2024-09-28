using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ApplyItemToggler(AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers, BlendTree root, List<InternalParameter> parameters)
            {
                foreach(var toggler in togglers)
                {
                    if(toggler.parameter.objects.Length + toggler.parameter.blendShapeModifiers.Length + toggler.parameter.materialReplacers.Length + toggler.parameter.materialPropertyModifiers.Length + toggler.parameter.clips.Length > 0)
                    {
                        // コンポーネントの設定値とprefab初期値を取得したAnimationClipを作成
                        var clips = toggler.parameter.CreateClip(ctx.AvatarRootObject, toggler.menuName);
                        var (clipDefault, clipChanged) = (clips.clipDefault.ToClip(), clips.clipChanged.ToClip());
                        AssetDatabase.AddObjectToAsset(clipDefault, ctx.AssetContainer);
                        AssetDatabase.AddObjectToAsset(clipChanged, ctx.AssetContainer);

                        // AnimatorControllerに追加
                        if(root) AnimationHelper.AddItemTogglerTree(controller, clipDefault, clipChanged, toggler.menuName, toggler.parameterName, toggler.defaultValue, root);
                        else AnimationHelper.AddItemTogglerLayer(controller, hasWriteDefaultsState, clipDefault, clipChanged, toggler.menuName, toggler.parameterName, toggler.defaultValue);
                    }
                    else
                    {
                        controller.TryAddParameter(toggler.parameterName, toggler.defaultValue);
                    }

                    parameters.Add(new InternalParameter(toggler.parameterName, toggler.defaultValue ? 1 : 0, toggler.isLocalOnly, toggler.isSave, InternalParameterType.Bool));
                }
            }
        }
    }
}
