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
        internal static void ApplyItemToggler(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, ItemToggler[] togglers
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu, VRCExpressionParameters parameters, Dictionary<MenuFolder, VRCExpressionsMenu> dic, BlendTree root
        #endif
        )
        {
            foreach(var toggler in togglers)
            {
                var name = toggler.menuName;
                if(toggler.parameter.objects.Length + toggler.parameter.blendShapeModifiers.Length + toggler.parameter.materialReplacers.Length + toggler.parameter.materialPropertyModifiers.Length > 0)
                {
                    var clips = toggler.parameter.CreateClip(ctx, name);
                    AssetDatabase.AddObjectToAsset(clips.Item1, ctx.AssetContainer);
                    AssetDatabase.AddObjectToAsset(clips.Item2, ctx.AssetContainer);
                    if(root) AnimationHelper.AddSimpleTree(controller, clips.Item1, clips.Item2, name, root);
                    else AnimationHelper.AddSimpleLayer(controller, hasWriteDefaultsState, clips.Item1, clips.Item2, name);
                }
                if(!root && !controller.parameters.Any(p => p.name == name))
                    controller.AddParameter(name, AnimatorControllerParameterType.Bool);

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
