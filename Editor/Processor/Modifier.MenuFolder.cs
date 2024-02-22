using System.Collections.Generic;
using jp.lilxyzw.avatarmodifier.runtime;
using UnityEditor;
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
        internal static void ApplyMenuFolder(BuildContext ctx, MenuFolder[] folders
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu, Dictionary<MenuFolder, VRCExpressionsMenu> dic
        #endif
        )
        {
            foreach(var folder in folders)
            {
                #if LIL_VRCSDK3A
                TryGetMenuFolder(ctx, folder, menu, dic);
                #endif
            }
        }

        #if LIL_VRCSDK3A
        internal static VRCExpressionsMenu TryGetMenuFolder(BuildContext ctx, MenuFolder folder, VRCExpressionsMenu root, Dictionary<MenuFolder, VRCExpressionsMenu> dic)
        {
            if(dic.ContainsKey(folder)) return dic[folder];

            var parentMenu = root;
            var parent = folder.GetMenuParent();
            if(parent) parentMenu = TryGetMenuFolder(ctx, parent, root, dic);
            if(parentMenu.controls.Count >= VRCExpressionsMenu.MAX_CONTROLS)
                throw new System.Exception("Menu Over!!!!!!!");

            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            menu.name = folder.menuName;
            AssetDatabase.AddObjectToAsset(menu, ctx.AssetContainer);
            parentMenu.AddMenu(menu);
            return dic[folder] = menu;
        }
        #endif
    }
}
