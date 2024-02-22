using System;
using System.Linq;
using jp.lilxyzw.avatarmodifier.runtime;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
{
    internal static partial class ObjHelper
    {
        private static string GetMenuName(this MenuBaseComponent component)
        {
            var name = component.menuName;
            if(string.IsNullOrEmpty(name)) name = component.gameObject.name;
            return name;
        }

        private static string GetMenuName(this Costume costume)
        {
            if(!string.IsNullOrEmpty(costume.menuName)) return costume.menuName;
            string name = costume.parametersPerMenu.objects.Select(o => o.obj).First(o => o && !string.IsNullOrEmpty(o.name)).name;
            return name;
        }

        internal static void ResolveMenuName(this MenuBaseComponent[] components)
        {
            foreach(var component in components)
                component.menuName = component.GetMenuName();
        }

        internal static void ResolveMenuName(this CostumeChanger[] changers)
        {
            foreach(var changer in changers)
                foreach(var costume in changer.costumes)
                    costume.menuName = costume.GetMenuName();
        }

        internal static MenuFolder GetMenuParent(this MenuBaseComponent component)
        {
            if(component.parentOverride) return component.parentOverride;
            return component.gameObject.GetComponentInParentInAvatar<MenuFolder>();
        }

        internal static void CheckApplyToAll(ItemToggler[] togglers, CostumeChanger[] costumeChangers, SmoothChanger[] smoothChangers)
        {
            foreach(var toggler in togglers) toggler.parameter.CheckApplyToAll();
            foreach(var changer in costumeChangers)
                foreach(var costume in changer.costumes) costume.parametersPerMenu.CheckApplyToAll();
            foreach(var changer in smoothChangers)
                foreach(var frame in changer.frames) frame.parametersPerMenu.CheckApplyToAll();
        }

        private static void CheckApplyToAll(this ParametersPerMenu parameters)
        {
            foreach(var modifier in parameters.blendShapeModifiers) modifier.CheckApplyToAll();
        }

        private static void CheckApplyToAll(this BlendShapeModifier modifier)
        {
            modifier.applyToAll = !modifier.skinnedMeshRenderer;
        }
    }
}
