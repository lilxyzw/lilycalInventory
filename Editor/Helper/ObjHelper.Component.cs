using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
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
            var duplicates = components.Where(c => !(c is MenuFolder)).GroupBy(c => c.menuName).Where(g => g.Count() > 1).SelectMany(g => g).ToArray();
            if(duplicates.Length > 0) ErrorHelper.Report("dialog.error.menuNameDuplication", duplicates);
        }

        internal static void ResolveMenuName(this CostumeChanger[] changers)
        {
            foreach(var changer in changers)
                foreach(var costume in changer.costumes)
                    costume.menuName = costume.GetMenuName(changer);

            var objs = changers.Where(c => c.costumes.Any(d => string.IsNullOrEmpty(d.menuName)));
            if(objs.Count() > 0) ErrorHelper.Report("dialog.error.menuNameEmpty", objs.ToArray());
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

        internal static void CheckUseless(ItemToggler[] togglers)
        {
            foreach(var toggler in togglers) toggler.parameter.CheckUseless();
        }

        private static void CheckUseless(this ParametersPerMenu parameters)
        {
            parameters.objects = parameters.objects.Where(t => t.obj.activeSelf != t.value).ToArray();
        }

        private static ParametersPerMenu Clone(this ParametersPerMenu parameters)
        {
            var p = new ParametersPerMenu();
            if(parameters != null)
            {
                if(parameters.objects != null) p.objects = (ObjectToggler[])parameters.objects.Clone();
                if(parameters.blendShapeModifiers != null) p.blendShapeModifiers = (BlendShapeModifier[])parameters.blendShapeModifiers.Clone();
                if(parameters.materialReplacers != null) p.materialReplacers = (MaterialReplacer[])parameters.materialReplacers.Clone();
                if(parameters.materialPropertyModifiers != null) p.materialPropertyModifiers = (MaterialPropertyModifier[])parameters.materialPropertyModifiers.Clone();
            }
            if(p.objects == null) p.objects = new ObjectToggler[]{};
            if(p.blendShapeModifiers == null) p.blendShapeModifiers = new BlendShapeModifier[]{};
            if(p.materialReplacers == null) p.materialReplacers = new MaterialReplacer[]{};
            if(p.materialPropertyModifiers == null) p.materialPropertyModifiers = new MaterialPropertyModifier[]{};
            return p;
        }

        internal static Costume[] DresserToCostumes(this AutoDresser[] dressers, out Transform avatarRoot, AutoDresser dresserDefOverride = null)
        {
            avatarRoot = null;
            if(dressers == null || dressers.Length == 0) return null;
            Costume def = null;
            var costumes = new List<Costume>();
            foreach(var dresser in dressers)
            {
                var obj = dresser.gameObject;
                if(avatarRoot == null) avatarRoot = obj.GetAvatarRoot();
                var cos = new Costume{
                    menuName = dresser.menuName,
                    icon = dresser.icon,
                    parentOverride = dresser.parentOverride,
                    parentOverrideMA = dresser.parentOverrideMA,
                    parametersPerMenu = dresser.parameter.Clone()
                };
                cos.parametersPerMenu.objects = cos.parametersPerMenu.objects.Append(new ObjectToggler{obj = obj, value = true}).ToArray();
                if(dresserDefOverride && dresser == dresserDefOverride) def = cos;
                else if(!dresserDefOverride && obj.activeSelf)
                {
                    if(def == null) def = cos;
                    else ErrorHelper.Report("dialog.error.defaultDuplication", dressers);
                }
                else costumes.Add(cos);
            }
            if(def == null) ErrorHelper.Report("dialog.error.allObjectOff", dressers);
            if(!avatarRoot) ErrorHelper.Report("dialog.error.avatarRootNofFound", dressers);
            costumes.Insert(0, def);
            return costumes.ToArray();
        }

        internal static void DresserToChanger(this AutoDresser[] dressers, AutoDresserSettings[] settings)
        {
            if(dressers == null || dressers.Length == 0) return;
            if(settings.Length == 1 && settings[0])
            {
                var s = settings[0];
                var obj = s.gameObject;
                var menuName = s.menuName;
                var icon = s.icon;
                var parentOverride = s.parentOverride;
                var parentOverrideMA = s.parentOverrideMA;
                Object.DestroyImmediate(s);
                var changer = obj.AddComponent<CostumeChanger>();
                changer.menuName = menuName;
                changer.icon = icon;
                changer.parentOverride = parentOverride;
                changer.parentOverrideMA = parentOverrideMA;
                changer.costumes = dressers.DresserToCostumes(out Transform avatarRoot);
            }
            else
            {
                var newObj = new GameObject(nameof(AutoDresser));
                var changer = newObj.AddComponent<CostumeChanger>();
                changer.menuName = nameof(AutoDresser);
                changer.costumes = dressers.DresserToCostumes(out Transform avatarRoot);
                newObj.transform.parent = avatarRoot;
            }
        }

        internal static void PropToToggler(this Prop[] props)
        {
            if(props == null || props.Length == 0) return;
            foreach(var prop in props)
            {
                var obj = prop.gameObject;
                var toggler = prop.PropToToggler();
                Object.DestroyImmediate(prop);
                var toggler2 = obj.AddComponent<ItemToggler>();
                toggler2.menuName = toggler.menuName;
                toggler2.icon = toggler.icon;
                toggler2.isSave = toggler.isSave;
                toggler2.isLocalOnly = toggler.isLocalOnly;
                toggler2.parameter = toggler.parameter;
            }
        }

        internal static ItemToggler PropToToggler(this Prop prop)
        {
            var obj = prop.gameObject;
            var toggler = new ItemToggler{
                menuName = prop.menuName,
                icon = prop.icon,
                isSave = prop.isSave,
                isLocalOnly = prop.isLocalOnly,
                parameter = prop.parameter.Clone()
            };
            toggler.parameter.objects = prop.parameter.objects.Append(new ObjectToggler{obj = obj, value = !obj.activeSelf}).ToArray();
            return toggler;
        }
    }
}
