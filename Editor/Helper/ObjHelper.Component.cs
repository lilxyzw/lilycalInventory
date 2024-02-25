using System.Linq;

namespace jp.lilxyzw.avatarmodifier
{
    using runtime;

    internal static partial class ObjHelper
    {
        private static string GetMenuName(this MenuBaseComponent component)
        {
            var name = component.menuName;
            if(string.IsNullOrEmpty(name)) name = component.gameObject.name;
            return name;
        }

        private static string GetMenuName(this Costume costume, CostumeChanger changer)
        {
            if(!string.IsNullOrEmpty(costume.menuName)) return costume.menuName;
            var obj = costume.parametersPerMenu.objects.Select(o => o.obj).FirstOrDefault(o => o && !string.IsNullOrEmpty(o.name));
            if(obj && !string.IsNullOrEmpty(obj.name)) return obj.name;
            var shape = costume.parametersPerMenu.blendShapeModifiers.SelectMany(m => m.blendShapeNameValues).Select(v => v.name).FirstOrDefault(n => !string.IsNullOrEmpty(n));
            if(!string.IsNullOrEmpty(shape)) return shape;
            var material = costume.parametersPerMenu.materialReplacers.SelectMany(r => r.replaceTo).FirstOrDefault(m => m && !string.IsNullOrEmpty(m.name));
            if(material && !string.IsNullOrEmpty(material.name)) return material.name;
            var nameF = costume.parametersPerMenu.materialPropertyModifiers.SelectMany(m => m.floatModifiers).Select(m => m.propertyName).FirstOrDefault(n => !string.IsNullOrEmpty(n));
            if(!string.IsNullOrEmpty(nameF)) return nameF;
            var nameV = costume.parametersPerMenu.materialPropertyModifiers.SelectMany(m => m.vectorModifiers).Select(m => m.propertyName).FirstOrDefault(n => !string.IsNullOrEmpty(n));
            if(!string.IsNullOrEmpty(nameV)) return nameF;
            return null;
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
                    costume.menuName = costume.GetMenuName(changer);

            var objs = changers.Where(c => c.costumes.Any(d => string.IsNullOrEmpty(d.menuName)));
            if(objs.Count() > 0) ErrorHelper.Report("dialog.error.menunameEmpty", objs.ToArray());
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
            foreach(var blendShapeModifier in parameters.blendShapeModifiers) 
                blendShapeModifier.applyToAll = !blendShapeModifier.skinnedMeshRenderer;
        }
    }
}
