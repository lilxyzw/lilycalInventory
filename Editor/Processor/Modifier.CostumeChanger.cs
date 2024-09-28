using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_VRCSDK3A
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ApplyCostumeChanger(AnimatorController controller, bool hasWriteDefaultsState, CostumeChanger[] changers, BlendTree root, List<InternalParameter> parameters)
            {
                foreach(var changer in changers)
                {
                    var costumeCount = changer.costumes.Length;
                    var bits = ObjHelper.ToNBitInt(costumeCount);
                    if(costumeCount != 0)
                    {
                        var clipDefaults = new InternalClip[changer.costumes.Length];
                        var clipChangeds = new InternalClip[changer.costumes.Length];

                        // 各衣装の設定値とprefab初期値を取得したAnimationClipを作成
                        for(int i = 0; i < changer.costumes.Length; i++)
                        {
                            var costume = changer.costumes[i];
                            (clipDefaults[i], clipChangeds[i]) = costume.parametersPerMenu.CreateClip(ctx.AvatarRootObject, costume.menuName);
                        }

                        // 同期事故防止のためにオブジェクトのオンオフ状況をコンポーネントの設定に合わせる
                        foreach(var toggler in changer.costumes[changer.defaultValue].parametersPerMenu.objects)
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
                        if(root) AnimationHelper.AddCostumeChangerTree(controller, clips, changer.menuName, changer.parameterName, changer.defaultValue, root);
                        else AnimationHelper.AddCostumeChangerLayer(controller, hasWriteDefaultsState, clips, changer.menuName, changer.parameterName, changer.defaultValue);
                    }
                    else
                    {
                        controller.TryAddParameter(changer.parameterName, changer.defaultValue);
                    }

                    #if LIL_VRCSDK3A
                    // パラメーターを追加
                    if(changer.isLocalOnly)
                    {
                        parameters.Add(new InternalParameter(changer.parameterName, changer.defaultValue, changer.isLocalOnly, changer.isSave, InternalParameterType.Int));
                    }
                    else
                    {
                        // Localでつかうintと同期されるboolを生成
                        controller.TryAddParameter(changer.parameterNameLocal, changer.defaultValue);
                        parameters.Add(new InternalParameter(changer.parameterNameLocal, changer.defaultValue, true, changer.isSave, InternalParameterType.Int));
                        for(int bit = 0; bit < bits; bit++)
                        {
                            bool defaultValue = (changer.defaultValue & 1 << bit) != 0;
                            controller.TryAddParameter(changer.parameterNameBits[bit], defaultValue);
                            parameters.Add(new InternalParameter(changer.parameterNameBits[bit], defaultValue ? 1 : 0, false, changer.isSave, InternalParameterType.Bool));
                        }

                        // 空のClip
                        var emptyClip = new AnimationClip(){name = "Empty"};
                        AssetDatabase.AddObjectToAsset(emptyClip, ctx.AssetContainer);

                        var stateMachineComp = new AnimatorStateMachine();
                        var stateMachineDecomp = new AnimatorStateMachine();
                        for(int i = 0; i < costumeCount; i++)
                        {
                            // 圧縮用
                            var stateComp = new AnimatorState
                            {
                                motion = emptyClip,
                                name = $"{changer.menuName}To{i}",
                                writeDefaultValues = hasWriteDefaultsState
                            };

                            var driverComp = stateComp.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                            driverComp.localOnly = true; // 圧縮はローカルでいい

                            stateMachineComp.AddState(stateComp, stateMachineComp.entryPosition + new Vector3(200,costumeCount*25-i*50,0));
                            if(i == changer.defaultValue) stateMachineComp.defaultState = stateComp;

                            var transitionToStateComp = stateMachineComp.AddEntryTransition(stateComp);
                            transitionToStateComp.AddCondition(AnimatorConditionMode.Equals, i, changer.parameterNameLocal);
                            var transitionToExitComp = stateComp.AddExitTransition();
                            transitionToExitComp.AddCondition(AnimatorConditionMode.NotEqual, i, changer.parameterNameLocal);
                            transitionToExitComp.duration = 0;

                            // 展開用
                            var stateDecomp = new AnimatorState
                            {
                                motion = emptyClip,
                                name = $"{changer.menuName}To{i}",
                                writeDefaultValues = hasWriteDefaultsState
                            };

                            var driverDecomp = stateDecomp.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                            driverDecomp.localOnly = false; // 展開はグローバル
                            driverDecomp.parameters.Add(new VRC_AvatarParameterDriver.Parameter(){
                                type = VRC_AvatarParameterDriver.ChangeType.Set,
                                name = changer.menuName,
                                value = i
                            });

                            stateMachineDecomp.AddState(stateDecomp, stateMachineDecomp.entryPosition + new Vector3(200,costumeCount*25-i*50,0));
                            if(i == changer.defaultValue) stateMachineDecomp.defaultState = stateDecomp;

                            var transitionToStateDecomp = stateMachineDecomp.AddEntryTransition(stateDecomp);

                            // ビットごとの処理
                            for(int bit = 0; bit < bits; bit++)
                            {
                                var boolValue = (i & 1 << bit) != 0;
                                driverComp.parameters.Add(new VRC_AvatarParameterDriver.Parameter(){
                                    type = VRC_AvatarParameterDriver.ChangeType.Set,
                                    name = changer.parameterNameBits[bit],
                                    value = boolValue ? 1 : 0
                                });

                                transitionToStateDecomp.AddCondition(boolValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, changer.parameterNameBits[bit]);
                                var transitionToExitDecomp = stateDecomp.AddExitTransition();
                                transitionToExitDecomp.AddCondition(!boolValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, changer.parameterNameBits[bit]);
                                transitionToExitDecomp.duration = 0;
                            }
                        }

                        var layerComp = new AnimatorControllerLayer
                        {
                            blendingMode = AnimatorLayerBlendingMode.Override,
                            defaultWeight = 1,
                            name = $"{changer.menuName}_Comp",
                            stateMachine = stateMachineComp
                        };
                        controller.AddLayer(layerComp);

                        var layerDecomp = new AnimatorControllerLayer
                        {
                            blendingMode = AnimatorLayerBlendingMode.Override,
                            defaultWeight = 1,
                            name = $"{changer.menuName}_Decomp",
                            stateMachine = stateMachineDecomp
                        };
                        controller.AddLayer(layerDecomp);
                    }
                    #else
                    parameters.Add(new InternalParameter(changer.parameterName, changer.defaultValue, changer.isLocalOnly, changer.isSave, InternalParameterType.Int));
                    #endif
                }
            }
        }
    }
}
