using System.Collections.Generic;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace jp.lilxyzw.avatarmodifier
{
    using runtime;

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
            var menu = VRChatHelper.CreateMenu(ctx, folder.menuName);
            parentMenu.AddMenu(menu, folder);
            return dic[folder] = menu;
        }
        #endif
    }
}
