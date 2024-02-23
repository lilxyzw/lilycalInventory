using System.Collections.Generic;
using System.Linq;
using jp.lilxyzw.avatarmodifier.runtime;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace jp.lilxyzw.avatarmodifier
{
    internal partial class Modifier
    {
        internal static void ApplyCostumeChanger(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, CostumeChanger[] changers
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu, VRCExpressionParameters parameters, Dictionary<MenuFolder, VRCExpressionsMenu> dic
        #endif
        )
        {
            foreach(var changer in changers)
            {
                if(changer.costumes.Length == 0) continue;
                var name = changer.menuName;
                var clipDefaults = new AnimationClip[changer.costumes.Length];
                var clipChangeds = new AnimationClip[changer.costumes.Length];
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
                var clipDefault = AnimationHelper.MergeClips(clipDefaults);
                for(int i = 0; i < clipChangeds.Length; i++)
                {
                    clipChangeds[i] = AnimationHelper.MergeAndCreate(clipChangeds[i], clipDefault);
                    clipChangeds[i].name = $"{changer.costumes[i].menuName}_Merged";
                    AssetDatabase.AddObjectToAsset(clipChangeds[i], ctx.AssetContainer);
                }
                AddCostumeChangerLayer(controller, hasWriteDefaultsState, clipChangeds, name);

                #if LIL_VRCSDK3A
                var parentMenu = menu;
                var parent = changer.GetMenuParent();
                if(parent && dic.ContainsKey(parent)) parentMenu = dic[parent];

                var costumeMenu = AddFolder(ctx, changer, menu, dic);
                for(int i = 0; i < clipChangeds.Length; i++)
                {
                    var control = new VRCExpressionsMenu.Control
                    {
                        icon = changer.costumes[i].icon,
                        name = changer.costumes[i].menuName,
                        parameter = new VRCExpressionsMenu.Control.Parameter{name = name},
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        value = i
                    };
                    costumeMenu.controls.Add(control);
                }
                parameters.AddParameterInt(name, changer.isLocalOnly, changer.isSave);
                #endif
            }
        }

        private static void AddCostumeChangerLayer(AnimatorController controller, bool hasWriteDefaultsState, AnimationClip[] clips, string name)
        {
            var stateMachine = new AnimatorStateMachine();

            for(int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                var state = new AnimatorState
                {
                    motion = clip,
                    name = clip.name,
                    writeDefaultValues = hasWriteDefaultsState
                };
                stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(200,clips.Length*25-i*50,0));
                stateMachine.AddEntryTransition(state).AddCondition(AnimatorConditionMode.Equals, i, name);
                var toExit = state.AddExitTransition();
                toExit.AddCondition(AnimatorConditionMode.NotEqual, i, name);
                toExit.duration = 0;
            }

            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = name,
                stateMachine = stateMachine
            };

            controller.AddLayer(layer);
            if(!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, AnimatorControllerParameterType.Int);
        }

        #if LIL_VRCSDK3A
        private static VRCExpressionsMenu AddFolder(BuildContext ctx, CostumeChanger changer, VRCExpressionsMenu root, Dictionary<MenuFolder, VRCExpressionsMenu> dic)
        {
            var parentMenu = root;
            var parent = changer.GetMenuParent();
            if(parent) parentMenu = dic[parent];
            var menu = VRChatHelper.CreateMenu(ctx, changer.menuName);
            parentMenu.AddMenu(menu, changer);
            return menu;
        }
        #endif
    }
}
