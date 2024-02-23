using System.Collections.Generic;
using jp.lilxyzw.avatarmodifier.runtime;
using UnityEditor;
using UnityEditor.Animations;

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
        internal static void ApplyItemToggler(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu, VRCExpressionParameters parameters, Dictionary<MenuFolder, VRCExpressionsMenu> dic
        #endif
        )
        {
            foreach(var toggler in togglers)
            {
                var name = toggler.menuName;
                var clips = toggler.parameter.CreateClip(ctx, name);
                AssetDatabase.AddObjectToAsset(clips.Item1, ctx.AssetContainer);
                AssetDatabase.AddObjectToAsset(clips.Item2, ctx.AssetContainer);
                AnimationHelper.AddSimpleLayer(controller, hasWriteDefaultsState, clips.Item1, clips.Item2, name);

                #if LIL_VRCSDK3A
                var parentMenu = menu;
                var parent = toggler.GetMenuParent();
                if(parent && dic.ContainsKey(parent)) parentMenu = dic[parent];
                parentMenu.controls.Add(toggler.GetMenuControlToggle());
                parameters.AddParameterToggle(name, toggler.isLocalOnly, toggler.isSave);
                #endif
            }
        }
    }
}
