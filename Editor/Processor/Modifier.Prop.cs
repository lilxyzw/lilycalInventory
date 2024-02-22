using System.Collections.Generic;
using jp.lilxyzw.materialmodifier.runtime;
using UnityEditor;
using UnityEditor.Animations;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace jp.lilxyzw.materialmodifier
{
    internal partial class Modifier
    {
        internal static void ApplyProp(BuildContext ctx, AnimatorController controller, bool hasWriteDefaultsState, Prop[] props
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu, VRCExpressionParameters parameters, Dictionary<MenuFolder, VRCExpressionsMenu> dic
        #endif
        )
        {
            foreach(var prop in props)
            {
                var name = prop.menuName;
                var clips = prop.CreateClip(name);
                AssetDatabase.AddObjectToAsset(clips.Item1, ctx.AssetContainer);
                AssetDatabase.AddObjectToAsset(clips.Item2, ctx.AssetContainer);
                AnimationHelper.AddSimpleLayer(controller, hasWriteDefaultsState, clips.Item1, clips.Item2, name);

                #if LIL_VRCSDK3A
                var parentMenu = menu;
                var parent = prop.GetMenuParent();
                if(parent && dic.ContainsKey(parent)) parentMenu = dic[parent];

                if(parentMenu.controls.Count >= VRCExpressionsMenu.MAX_CONTROLS)
                    throw new System.Exception("Menu Over!!!!!!!");
                parentMenu.controls.Add(prop.GetMenuControlToggle());
                parameters.AddParameterToggle(name, prop.isLocalOnly, prop.isSave);
                #endif
            }
        }
    }
}
