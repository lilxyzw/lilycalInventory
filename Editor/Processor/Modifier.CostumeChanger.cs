using System.Collections.Generic;
using System.Linq;
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
        , VRCExpressionsMenu menu, VRCExpressionParameters parameters, Dictionary<MenuFolder, VRCExpressionsMenu> dic
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
