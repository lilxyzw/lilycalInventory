using System;
using System.Collections.Generic;
using System.Linq;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private class MenuGenerator
        {
            // メニューの追加処理
            // 順序処理を追加するにあたって大幅な書き換えを予定
            internal static InternalMenu Generate(MenuFolder[] folders, ItemToggler[] togglers, SmoothChanger[] smoothChangers, CostumeChanger[] costumeChangers, Preset[] presets, MenuBaseComponent[] menuBaseComponents)
            {
                var root = new InternalMenu();
                var menus = new Dictionary<MenuBaseComponent, InternalMenu>();
                var controls = new Dictionary<MenuBaseComponent, List<(MenuBaseComponent parent, InternalMenu menu)>>();

                // 親フォルダを生成
                foreach(var folder in folders)
                {
                    if(folder.parentOverrideMA) continue;
                    menus[folder] = InternalMenu.CreateFolder(folder.menuName, folder.icon);
                    controls[folder] = new List<(MenuBaseComponent, InternalMenu)>()
                    {
                        (folder.GetMenuParent(), menus[folder])
                    };
                }

                // ItemTogglerを追加
                foreach(var toggler in togglers)
                {
                    if(toggler.parentOverrideMA) continue;
                    controls[toggler] = new List<(MenuBaseComponent, InternalMenu)>()
                    {
                        (toggler.GetMenuParent(), InternalMenu.CreateToggle(toggler.menuName, toggler.icon, toggler.parameterName))
                    };
                }

                // SmoothChangerを追加
                foreach(var changer in smoothChangers)
                {
                    if(changer.parentOverrideMA || changer.frames.Length == 0) continue;
                    controls[changer] = new List<(MenuBaseComponent, InternalMenu)>()
                    {
                        (changer.GetMenuParent(), InternalMenu.CreateSlider(changer.menuName, changer.icon, changer.parameterName))
                    };
                }

                // CostumeChangerを追加
                foreach(var changer in costumeChangers)
                {
                    if(changer.parentOverrideMA || changer.costumes.Length == 0) continue;

                    if(changer.costumes.Count(c => !c.parentOverride && !c.parentOverrideMA) > 0)
                    {
                        menus[changer] = InternalMenu.CreateFolder(changer.menuName, changer.icon);
                        controls[changer] = new List<(MenuBaseComponent, InternalMenu)>()
                        {
                            (changer.GetMenuParent(), menus[changer])
                        };
                    }

                    for(int i = 0; i < changer.costumes.Length; i++)
                    {
                        var costume = changer.costumes[i];
                        if(costume.parentOverrideMA) continue;
                        var parent = costume.parentOverride ? costume.parentOverride : changer as MenuBaseComponent;
                        if(!controls.ContainsKey(changer))
                        {
                            controls[changer] = new List<(MenuBaseComponent, InternalMenu)>();
                        }
                        controls[changer].Add((parent, InternalMenu.CreateToggle(costume.menuName, costume.icon, changer.isLocalOnly ? changer.parameterName : changer.parameterNameLocal, i)));
                    }
                }

                // Presetを追加
                foreach(var preset in presets)
                {
                    if(preset.parentOverrideMA) continue;
                    controls[preset] = new List<(MenuBaseComponent, InternalMenu)>()
                    {
                        (preset.GetMenuParent(), InternalMenu.CreateTrigger(preset.menuName, preset.icon, preset.parameterName))
                    };
                }

                // Hierarchy 順でソートしてメニューを構築
                foreach(var (parent, control) in controls
                    .OrderBy(x => x.Key, Comparer<MenuBaseComponent>.Create((a, b) => Array.IndexOf(menuBaseComponents, a) - Array.IndexOf(menuBaseComponents, b)))
                    .SelectMany(x => x.Value))
                {
                    (parent ? menus[parent] : root).menus.Add(control);
                }

                // 循環参照を検出
                var childFolders = menus.Keys
                    .OfType<MenuFolder>()
                    .GroupBy(x => x.GetMenuParent())
                    .Where(x => x.Key != null)
                    .ToDictionary(x => x.Key, x => x.ToArray());
                var circularFolders = menus.Keys
                    .OfType<MenuFolder>()
                    .SelectMany(x => FindCircularFolders(x))
                    .ToArray();
                if(circularFolders.Length > 0)
                {
                    ErrorHelper.Report("dialog.error.menuCircularReference", circularFolders);
                }

                IEnumerable<MenuFolder> FindCircularFolders(MenuFolder root, MenuFolder current = null)
                {
                    var folder = current == null ? root : current;
                    if(childFolders.ContainsKey(folder))
                    {
                        foreach(var childFolder in childFolders[folder])
                        {
                            if(childFolder == root)
                            {
                                yield return childFolder;
                            }
                            else
                            {
                                foreach(var circularFolder in FindCircularFolders(root, childFolder))
                                {
                                    yield return circularFolder;
                                }
                            }
                        }
                    }
                }

                return root;
            }
        }
    }
}
