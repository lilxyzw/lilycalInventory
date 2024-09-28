using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ApplySmoothChanger(AnimatorController controller, bool hasWriteDefaultsState, SmoothChanger[] changers, BlendTree root, List<InternalParameter> parameters)
            {
                foreach(var changer in changers)
                {
                    if(changer.frames.Length != 0)
                    {
                        var clipDefaults = new InternalClip[changer.frames.Length];
                        var clipChangeds = new InternalClip[changer.frames.Length];
                        var frames = new float[changer.frames.Length];

                        // 各フレームの設定値とprefab初期値を取得したAnimationClipを作成
                        for(int i = 0; i < changer.frames.Length; i++)
                        {
                            var frame = changer.frames[i];
                            var frameValue = Mathf.Clamp01(frame.frameValue);
                            var clip2 = frame.parametersPerMenu.CreateClip(ctx.AvatarRootObject, $"{changer.menuName}_{i}");
                            clipDefaults[i] = clip2.Item1;
                            clipChangeds[i] = clip2.Item2;
                            frames[i] = frameValue;
                        }

                        // prefab初期値AnimationClipをマージ
                        var clipDefault = InternalClip.MergeAndCreate(clipDefaults);

                        // 各フレームの未設定値をprefab初期値で埋める
                        var clips = new AnimationClip[clipChangeds.Length];
                        for(int i = 0; i < clipChangeds.Length; i++)
                        {
                            clipChangeds[i] = InternalClip.MergeAndCreate(clipChangeds[i], clipDefault);
                            clipChangeds[i].name = $"{changer.menuName}_{i}_Merged";
                            clips[i] = clipChangeds[i].ToClip();
                            AssetDatabase.AddObjectToAsset(clips[i], ctx.AssetContainer);
                        }

                        // AnimatorControllerに追加
                        if(root) AnimationHelper.AddSmoothChangerTree(controller, clips, frames, changer.menuName, changer.parameterName, changer.defaultFrameValue, root);
                        else AnimationHelper.AddSmoothChangerLayer(controller, hasWriteDefaultsState, clips, frames, changer.menuName, changer.parameterName, changer.defaultFrameValue);
                    }
                    else
                    {
                        controller.TryAddParameter(changer.parameterName, changer.defaultFrameValue);
                    }

                    parameters.Add(new InternalParameter(changer.parameterName, changer.defaultFrameValue, changer.isLocalOnly, changer.isSave, InternalParameterType.Float));
                }
            }
        }
    }
}
