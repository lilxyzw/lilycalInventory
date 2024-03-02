using System.Collections.Generic;
using System.Linq;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal class MenuGenerator
    {
        internal static void Generate(BuildContext ctx, MenuFolder[] folders, ItemToggler[] togglers, SmoothChanger[] smoothChangers, CostumeChanger[] costumeChangers
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu, Dictionary<MenuFolder, VRCExpressionsMenu> dic
        #endif
        )
        {
            #if LIL_VRCSDK3A
            VRCExpressionsMenu FindParent(MenuBaseComponent component)
            {
                var parent = component.GetMenuParent();
                if(parent && dic.ContainsKey(parent)) return dic[parent];
                return menu;
            }

            foreach(var folder in folders)
            {
                if(folder.parentOverrideMA) continue;
                TryGetMenuFolder(ctx, folder, menu, dic);
            }

            foreach(var toggler in togglers)
            {
                if(toggler.parentOverrideMA) continue;
                FindParent(toggler).controls.CreateAndAdd(toggler.menuName, toggler.icon, ControlType.Toggle, toggler.menuName);
            }

            foreach(var changer in smoothChangers)
            {
                if(changer.parentOverrideMA || changer.frames.Length == 0) continue;
                FindParent(changer).controls.CreateAndAdd(changer.menuName, changer.icon, ControlType.RadialPuppet, changer.menuName);
            }

            foreach(var changer in costumeChangers)
            {
                if(changer.parentOverrideMA || changer.costumes.Length == 0) continue;

                VRCExpressionsMenu costumeMenu = null;
                var parentMenu = FindParent(changer);

                if(changer.costumes.Count(c => !c.parentOverride && !c.parentOverrideMA) > 0)
                {
                    costumeMenu = VRChatHelper.CreateMenu(ctx, changer.menuName);
                    parentMenu.AddMenu(costumeMenu, changer);
                }
                for(int i = 0; i < changer.costumes.Length; i++)
                {
                    var costume = changer.costumes[i];
                    if(costume.parentOverrideMA) continue;
                    var parent = costumeMenu;
                    if(costume.parentOverride) parent = dic[costume.parentOverride];
                    parent.controls.CreateAndAdd(costume.menuName, costume.icon, ControlType.Toggle, changer.menuName, i);
                }
            }
            #endif
        }

        #if LIL_VRCSDK3A
        private static VRCExpressionsMenu TryGetMenuFolder(BuildContext ctx, MenuFolder folder, VRCExpressionsMenu root, Dictionary<MenuFolder, VRCExpressionsMenu> dic)
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
