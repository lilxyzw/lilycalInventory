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
                var name = changer.menuName;
                var costumeCount = changer.costumes.Length;
                var bits = 0;
                var n = costumeCount;
                while(n > 0){
                    bits++;
                    n >>= 1;
                }
                if(costumeCount != 0)
                {
                    var clipDefaults = new InternalClip[changer.costumes.Length];
                    var clipChangeds = new InternalClip[changer.costumes.Length];

                    // 各衣装の設定値とprefab初期値を取得したAnimationClipを作成
                    for(int i = 0; i < changer.costumes.Length; i++)
                    {
                        var costume = changer.costumes[i];
                        (clipDefaults[i], clipChangeds[i]) = costume.parametersPerMenu.CreateClip(ctx, costume.menuName);
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
                    if(root) AnimationHelper.AddCostumeChangerTree(controller, clips, name, changer.defaultValue, root);
                    else AnimationHelper.AddCostumeChangerLayer(controller, hasWriteDefaultsState, clips, name, changer.defaultValue);
                }
                else
                {
                    controller.TryAddParameter(name, changer.defaultValue);
                }

                #if LIL_VRCSDK3A
                // パラメーターを追加
                if(changer.isLocalOnly)
                {
                    parameters.AddParameterInt(name, true, changer.isSave, changer.defaultValue);
                }
                else
                {
                    // Localでつかうintと同期されるboolを生成
                    var localName = $"{name}_Local";
                    controller.TryAddParameter(localName, changer.defaultValue);
                    parameters.AddParameterInt(localName, true, changer.isSave, changer.defaultValue);
                    for(int bit = 0; bit < bits; bit++)
                    {
                        bool defaultValue = (changer.defaultValue & 1 << bit) != 0;
                        controller.TryAddParameter($"{name}_Bool{bit}", defaultValue);
                        parameters.AddParameterToggle($"{name}_Bool{bit}", false, changer.isSave, defaultValue);
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
                            name = $"{name}To{i}",
                            writeDefaultValues = hasWriteDefaultsState
                        };

                        var driverComp = stateComp.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                        driverComp.localOnly = true; // 圧縮はローカルでいい

                        stateMachineComp.AddState(stateComp, stateMachineComp.entryPosition + new Vector3(200,costumeCount*25-i*50,0));
                        if(i == changer.defaultValue) stateMachineComp.defaultState = stateComp;

                        var transitionToStateComp = stateMachineComp.AddEntryTransition(stateComp);
                        transitionToStateComp.AddCondition(AnimatorConditionMode.Equals, i, localName);
                        var transitionToExitComp = stateComp.AddExitTransition();
                        transitionToExitComp.AddCondition(AnimatorConditionMode.NotEqual, i, localName);
                        transitionToExitComp.duration = 0;

                        // 展開用
                        var stateDecomp = new AnimatorState
                        {
                            motion = emptyClip,
                            name = $"{name}To{i}",
                            writeDefaultValues = hasWriteDefaultsState
                        };

                        var driverDecomp = stateDecomp.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                        driverDecomp.localOnly = false; // 展開はグローバル
                        driverDecomp.parameters.Add(new VRC_AvatarParameterDriver.Parameter(){
                            type = VRC_AvatarParameterDriver.ChangeType.Set,
                            name = name,
                            value = i
                        });

                        stateMachineDecomp.AddState(stateDecomp, stateMachineDecomp.entryPosition + new Vector3(200,costumeCount*25-i*50,0));
                        if(i == changer.defaultValue) stateMachineDecomp.defaultState = stateDecomp;

                        var transitionToStateDecomp = stateMachineDecomp.AddEntryTransition(stateDecomp);

                        // ビットごとの処理
                        for(int bit = 0; bit < bits; bit++)
                        {
                            var boolName = $"{name}_Bool{bit}";
                            var boolValue = (i & 1 << bit) != 0;
                            driverComp.parameters.Add(new VRC_AvatarParameterDriver.Parameter(){
                                type = VRC_AvatarParameterDriver.ChangeType.Set,
                                name = boolName,
                                value = boolValue ? 1 : 0
                            });

                            transitionToStateDecomp.AddCondition(boolValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, boolName);
                            var transitionToExitDecomp = stateDecomp.AddExitTransition();
                            transitionToExitDecomp.AddCondition(!boolValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, boolName);
                            transitionToExitDecomp.duration = 0;
                        }
                    }

                    var layerComp = new AnimatorControllerLayer
                    {
                        blendingMode = AnimatorLayerBlendingMode.Override,
                        defaultWeight = 1,
                        name = $"{name}_Comp",
                        stateMachine = stateMachineComp
                    };
                    controller.AddLayer(layerComp);

                    var layerDecomp = new AnimatorControllerLayer
                    {
                        blendingMode = AnimatorLayerBlendingMode.Override,
                        defaultWeight = 1,
                        name = $"{name}_Decomp",
                        stateMachine = stateMachineDecomp
                    };
                    controller.AddLayer(layerDecomp);
                }
                #endif
            }
        }
    }
}
