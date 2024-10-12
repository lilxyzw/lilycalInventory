using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
#if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;
using Control = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;
using Parameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static class ModularAvatarHelper
    {
        internal static void Inspector(Object target, SerializedProperty iterator)
        {
            #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
            // MAのメニューで制御されている場合はその旨を伝えるヘルプボックスを表示
            if(iterator.objectReferenceValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(Localization.S("inspector.parentOverrideMAInfo"), MessageType.Info);
                EditorGUI.indentLevel--;
            }

            // そうでない場合かつオブジェクトがMenuGroup配下にある場合はMenuItemを生成
            else
            {
                var component = (MenuBaseComponent)target;
                if(component.gameObject.GetComponentInParentInAvatar<ModularAvatarMenuGroup>())
                {
                    var item = component.gameObject.GetComponent<ModularAvatarMenuItem>();
                    if(!item) item = component.gameObject.AddComponent<ModularAvatarMenuItem>();
                    item.MenuSource = SubmenuSource.Children;
                    item.Control = new Control{icon = component.icon};
                    switch(component)
                    {
                        case MenuFolder _:
                        case CostumeChanger _:
                            item.Control.type = ControlType.SubMenu;
                            break;
                        case SmoothChanger _:
                            item.Control.type = ControlType.RadialPuppet;
                            break;
                        default:
                            item.Control.type = ControlType.Button;
                            break;
                    }
                    iterator.objectReferenceValue = item;
                }
            }
            #endif
        }

        // 再帰的にMenuGroup配下に
        internal static void ResolveMenu(MenuFolder[] folders, ItemToggler[] togglers, CostumeChanger[] costumeChangers, SmoothChanger[] smoothChangers, Preset[] presets)
        {
            #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
            var menus = new Dictionary<ModularAvatarMenuItem, (Component,bool)>();
            var duplicates = new List<Component>();
            ResolveFolders(folders, menus, duplicates);
            foreach(var m in togglers) m.Resolve(ControlType.Toggle, menus, duplicates);
            foreach(var m in smoothChangers) m.Resolve(ControlType.RadialPuppet, menus, duplicates);
            foreach(var m in costumeChangers) m.Resolve(menus, duplicates);
            foreach(var m in presets) m.Resolve(ControlType.Toggle, menus, duplicates);

            // 複数コンポーネントから参照されるMenuItemがあった場合にエラー
            if(duplicates.Count > 0)
                ErrorHelper.Report("dialog.error.menuMADuplication", menus.Where(p => p.Value.Item2).Select(p => p.Value.Item1).Union(duplicates).ToArray());
            #else
            foreach(var m in folders) m.parentOverrideMA = null;
            foreach(var m in togglers) m.parentOverrideMA = null;
            foreach(var m in smoothChangers) m.parentOverrideMA = null;
            foreach(var m in costumeChangers)
            {
                m.parentOverrideMA = null;
                foreach(var c in m.costumes) c.parentOverrideMA = null;
            }
            foreach(var m in presets) m.parentOverrideMA = null;
            #endif
        }

        internal static void ToMergeAnimator(AsMAMergeAnimator asMAMergeAnimator, AnimatorController animatorController)
        {
            if(!asMAMergeAnimator) return;
            #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
            var mama = asMAMergeAnimator.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            mama.animator = animatorController;
            mama.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mama.deleteAttachedAnimator = false;
            mama.matchAvatarWriteDefaults = false;
            mama.pathMode = MergeAnimatorPathMode.Absolute;
            mama.layerPriority = asMAMergeAnimator.layerPriority;
            #endif
        }

        #if LIL_MODULAR_AVATAR && LIL_VRCSDK3A
        private static void TryAdd(this Dictionary<ModularAvatarMenuItem, (Component,bool)> menus, ModularAvatarMenuItem item, List<Component> duplicates, Component m)
        {
            if(menus.ContainsKey(item))
            {
                duplicates.Add(m);
                menus[item] = (menus[item].Item1,true);
            }
            else menus[item] = (m,false);
        }

        private static void ResolveFolders(MenuFolder[] folders, Dictionary<ModularAvatarMenuItem, (Component,bool)> menus, List<Component> duplicates)
        {
            var resolved = new HashSet<MenuFolder>();
            int resolvedCount = -1;
            while(resolvedCount != resolved.Count)
            {
                resolvedCount = resolved.Count;
                foreach(var m in folders)
                {
                    if(resolved.Contains(m)) continue;
                    if(m.parentOverrideMA)
                    {
                        m.parentOverrideMA.Control.type = ControlType.SubMenu;
                        m.parentOverrideMA.MenuSource = SubmenuSource.Children;
                        resolved.Add(m);
                        menus.TryAdd(m.parentOverrideMA, duplicates, m);
                    }
                    else
                    {
                        var parent = m.GetMenuParent();
                        if(parent && parent.parentOverrideMA)
                        {
                            m.parentOverrideMA = CreateChildMenuFolder(parent.parentOverrideMA.transform, m);
                            resolved.Add(m);
                            menus.TryAdd(m.parentOverrideMA, duplicates, m);
                        }
                    }
                }
            }
        }

        private static void Resolve(this MenuBaseComponent m, ControlType type, Dictionary<ModularAvatarMenuItem, (Component,bool)> menus, List<Component> duplicates)
        {
            if(!m.parentOverride) m.parentOverride = m.GetMenuParent();
            if(m.parentOverrideMA)
            {
                m.parentOverrideMA.Set(type, m.menuName);
                menus.TryAdd(m.parentOverrideMA, duplicates, m);
                return;
            }
            if(m.parentOverride && m.parentOverride.parentOverrideMA)
            {
                var parent = m.parentOverride.parentOverrideMA.gameObject.transform;
                m.parentOverrideMA = CreateChildMenu(parent, m, type);
                menus.TryAdd(m.parentOverrideMA, duplicates, m);
            }
        }

        private static void Resolve(this CostumeChanger m, Dictionary<ModularAvatarMenuItem, (Component,bool)> menus, List<Component> duplicates)
        {
            var parameterName = m.menuName;
            if(!m.isLocalOnly) parameterName += "_Local";
            if(!m.parentOverride) m.parentOverride = m.GetMenuParent();
            for(int i = 0; i < m.costumes.Length; i++)
            {
                var c = m.costumes[i];
                if(!c.parentOverrideMA && c.parentOverride && c.parentOverride.parentOverrideMA)
                {
                    c.parentOverrideMA = CreateChildMenu(c.parentOverride.parentOverrideMA.transform, c.menuName, c.icon, ControlType.Toggle, parameterName, i);
                }
                if(!c.parentOverrideMA) continue;
                c.parentOverrideMA.Set(ControlType.Toggle, parameterName, i);
                menus.TryAdd(c.parentOverrideMA, duplicates, m);
            }
            Transform parent;
            if(m.parentOverrideMA)
            {
                m.parentOverrideMA.Control.type = ControlType.SubMenu;
                m.parentOverrideMA.MenuSource = SubmenuSource.Children;
                menus.TryAdd(m.parentOverrideMA, duplicates, m);
                parent = m.parentOverrideMA.gameObject.transform;
            }
            else if(m.parentOverride && m.parentOverride.parentOverrideMA)
            {
                parent = m.parentOverride.parentOverrideMA.gameObject.transform;
            }
            else
            {
                return;
            }
            for(int i = 0; i < m.costumes.Length; i++)
            {
                var c = m.costumes[i];
                if(c.parentOverrideMA) continue;
                c.parentOverrideMA = CreateChildMenu(parent, c.menuName, c.icon, ControlType.Toggle, parameterName, i);
                menus.TryAdd(c.parentOverrideMA, duplicates, m);
            }
        }

        private static ModularAvatarMenuItem CreateChildMenuFolder(Transform parent, MenuBaseComponent m)
        {
            var o = new GameObject(m.menuName);
            o.transform.parent = parent;
            var menuItem = o.AddComponent<ModularAvatarMenuItem>();
            menuItem.MenuSource = SubmenuSource.Children;
            menuItem.Control = new Control{
                name = m.menuName,
                icon = m.icon,
                type = ControlType.SubMenu
            };
            return menuItem;
        }

        private static ModularAvatarMenuItem CreateChildMenu(Transform parent, string menuName, Texture2D icon, ControlType type, string parameterName, float value = 1)
        {
            var o = new GameObject(menuName);
            o.transform.parent = parent;
            var menuItem = o.AddComponent<ModularAvatarMenuItem>();
            menuItem.MenuSource = SubmenuSource.Children;
            menuItem.Control = new Control{
                name = menuName,
                icon = icon,
                type = type,
                value = value
            };
            if(type == ControlType.RadialPuppet) menuItem.Control.subParameters = new[]{new Parameter{name = parameterName}};
            else menuItem.Control.parameter = new Parameter{name = parameterName};
            return menuItem;
        }

        private static ModularAvatarMenuItem CreateChildMenu(Transform parent, MenuBaseComponent m, ControlType type, float value = 1)
        {
            return CreateChildMenu(parent, m.menuName, m.icon, type, m.menuName, value);
        }

        private static void Set(this ModularAvatarMenuItem item, ControlType type, string parameterName, float value = 1)
        {
            item.Control.type = type;
            item.Control.value = value;
            if(type == ControlType.RadialPuppet) item.Control.subParameters = new[]{new Parameter{name = parameterName}};
            else item.Control.parameter = new Parameter{name = parameterName};
        }
        #endif
    }
}
