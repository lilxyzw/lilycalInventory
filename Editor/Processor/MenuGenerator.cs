using System;
using System.Collections.Generic;
using System.Linq;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
using Control = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal class MenuGenerator
    {
        // メニューの追加処理
        // 順序処理を追加するにあたって大幅な書き換えを予定
        internal static void Generate(BuildContext ctx, MenuFolder[] folders, ItemToggler[] togglers, SmoothChanger[] smoothChangers, CostumeChanger[] costumeChangers
        , MenuBaseComponent[] menuBaseComponents
        #if LIL_VRCSDK3A
        , VRCExpressionsMenu menu
        #endif
        )
        {
            #if LIL_VRCSDK3A
            var menus = new Dictionary<MenuBaseComponent, VRCExpressionsMenu>();
            var controls = new Dictionary<MenuBaseComponent, List<(MenuBaseComponent parent, Control control)>>();

            // 親フォルダを生成
            foreach(var folder in folders)
            {
                if(folder.parentOverrideMA) continue;
                menus[folder] = VRChatHelper.CreateMenu(ctx, folder.menuName);
                controls[folder] = new List<(MenuBaseComponent, Control)>()
                {
                    (folder.GetMenuParent(), VRChatHelper.CreateControl(folder.menuName, folder.icon, menus[folder]))
                };
            }

            // ItemTogglerを追加
            foreach(var toggler in togglers)
            {
                if(toggler.parentOverrideMA) continue;
                controls[toggler] = new List<(MenuBaseComponent, Control)>()
                {
                    (toggler.GetMenuParent(), VRChatHelper.CreateControl(toggler.menuName, toggler.icon, ControlType.Toggle, toggler.menuName))
                };
            }

            // SmoothChangerを追加
            foreach(var changer in smoothChangers)
            {
                if(changer.parentOverrideMA || changer.frames.Length == 0) continue;
                controls[changer] = new List<(MenuBaseComponent, Control)>()
                {
                    (changer.GetMenuParent(), VRChatHelper.CreateControl(changer.menuName, changer.icon, ControlType.RadialPuppet, changer.menuName))
                };
            }

            // CostumeChangerを追加
            foreach(var changer in costumeChangers)
            {
                if(changer.parentOverrideMA || changer.costumes.Length == 0) continue;

                if(changer.costumes.Count(c => !c.parentOverride && !c.parentOverrideMA) > 0)
                {
                    menus[changer] = VRChatHelper.CreateMenu(ctx, changer.menuName);
                    controls[changer] = new List<(MenuBaseComponent, Control)>()
                    {
                        (changer.GetMenuParent(), VRChatHelper.CreateControl(changer.menuName, changer.icon, menus[changer]))
                    };
                }
                for(int i = 0; i < changer.costumes.Length; i++)
                {
                    var costume = changer.costumes[i];
                    if(costume.parentOverrideMA) continue;
                    var parent = costume.parentOverride ? costume.parentOverride : changer as MenuBaseComponent;
                    if(!controls.ContainsKey(changer))
                    {
                        controls[changer] = new List<(MenuBaseComponent, Control)>();
                    }
                    controls[changer].Add((parent, VRChatHelper.CreateControl(costume.menuName, costume.icon, ControlType.Toggle, changer.menuName, i)));
                }
            }

            // Hierarchy 順でソートしてメニューを構築
            foreach(var (parent, control) in controls
                .OrderBy(x => x.Key, Comparer<MenuBaseComponent>.Create((a, b) => Array.IndexOf(menuBaseComponents, a) - Array.IndexOf(menuBaseComponents, b)))
                .SelectMany(x => x.Value))
            {
                (parent != null ? menus[parent] : menu).controls.Add(control);
            }
            #endif
        }
    }
}
