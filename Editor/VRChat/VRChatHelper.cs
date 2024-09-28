using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

#if LIL_VRCSDK3A
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Control = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control;
using ControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;
using Parameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter;
using ValueType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType;
#endif

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    #if LIL_VRCSDK3A
    internal static class VRChatHelper
    {
        internal static int costMax = VRCExpressionParameters.MAX_PARAMETER_COST;
        internal static int costBool = VRCExpressionParameters.TypeCost(ValueType.Bool);
        internal static int costInt = VRCExpressionParameters.TypeCost(ValueType.Int);
        internal static int costFloat = VRCExpressionParameters.TypeCost(ValueType.Float);

        internal static void GetAnimatorControllers(VRCAvatarDescriptor descriptor, HashSet<RuntimeAnimatorController> controllers)
        {
            controllers.UnionWith(descriptor.specialAnimationLayers.Where(l => l.animatorController).Select(l => l.animatorController));
            if(descriptor.customizeAnimationLayers) controllers.UnionWith(descriptor.baseAnimationLayers.Where(l => l.animatorController).Select(l => l.animatorController));
        }

        internal static void GetAnimatorControllers(GameObject gameObject, HashSet<RuntimeAnimatorController> controllers)
        {
            var descriptor = gameObject.GetComponent<VRCAvatarDescriptor>();
            if(descriptor) GetAnimatorControllers(descriptor, controllers);
        }
    }

    internal static partial class Processor
    {
        private static class VRChatUtils
        {
            private static Texture2D m_IconNext;
            private static Texture2D iconNext => m_IconNext ? m_IconNext : m_IconNext = ObjHelper.LoadAssetByGUID<Texture2D>(ConstantValues.GUID_ICON_NEXT);

            internal static AnimatorController TryGetFXAnimatorController()
            {
                var descriptor = ctx.AvatarDescriptor;
                AnimatorController CreateFXController()
                {
                    var controller = new AnimatorController{name = "FX"};
                    AssetDatabase.AddObjectToAsset(controller, ctx.AssetContainer);
                    return controller;
                }

                // Layerのカスタマイズが無効の場合は有効化
                if(!descriptor.customizeAnimationLayers)
                {
                    descriptor.customizeAnimationLayers = true;
                }

                // FXレイヤーを見つける
                for(int i = 0; i < descriptor.baseAnimationLayers.Length; i++)
                {
                    if(descriptor.baseAnimationLayers[i].type != VRCAvatarDescriptor.AnimLayerType.FX) continue;
                    var layer = descriptor.baseAnimationLayers[i];
                    if(layer.isDefault)
                    {
                        layer.isDefault = false;
                        layer.animatorController = null;
                    }

                    // AnimatorControllerがセットされている場合はそれをクローンして返す
                    if(layer.animatorController)
                    {
                        var ac = AnimatorCombiner.DeepCloneAnimator(ctx, layer.animatorController);
                        layer.animatorController = ac;
                        return ac;
                    }

                    // 空の場合は新規に作ったものをセットしつつ返す
                    var controllerI = CreateFXController();
                    layer.animatorController = controllerI;
                    descriptor.baseAnimationLayers[i] = layer;
                    return controllerI;
                }

                // FXレイヤーがない場合は追加
                Array.Resize(ref descriptor.baseAnimationLayers, descriptor.baseAnimationLayers.Length+1);
                var newcontroller = CreateFXController();
                var newlayer = new VRCAvatarDescriptor.CustomAnimLayer
                {
                    animatorController = newcontroller,
                    isDefault = false,
                    type = VRCAvatarDescriptor.AnimLayerType.FX
                };
                descriptor.baseAnimationLayers[descriptor.baseAnimationLayers.Length - 1] = newlayer;
                return newcontroller;
            }

            internal static void Apply(InternalMenu rootMenu, List<InternalParameter> parameters)
            {
                var descriptor = ctx.AvatarDescriptor;
                // Expression未設定の場合は新規に設定
                if(!descriptor.customExpressions)
                {
                    descriptor.customExpressions = true;
                    descriptor.expressionsMenu = null;
                    descriptor.expressionParameters = null;
                }

                // クローン
                var map = new Dictionary<Object,Object>();
                ctx.AvatarDescriptor.expressionParameters = CloneObject(ctx.AvatarDescriptor.expressionParameters, ctx, map) as VRCExpressionParameters;
                ctx.AvatarDescriptor.expressionsMenu = CloneMenu(ctx.AvatarDescriptor.expressionsMenu, map, new HashSet<VRCExpressionsMenu>());

                // ExpressionsMenuが存在する場合はlilycalInventoryで生成したものとマージ
                foreach(var menu in rootMenu.menus) descriptor.expressionsMenu = InternalToExpressions(menu, descriptor.expressionsMenu);
                CombineSubMenu(descriptor.expressionsMenu, new HashSet<VRCExpressionsMenu>());
                ResolveOver(descriptor.expressionsMenu, new HashSet<VRCExpressionsMenu>());

                // ExpressionParametetsも同様
                descriptor.expressionParameters = InternalToExpressionParameters(parameters, descriptor.expressionParameters);
            }

            private static VRCExpressionsMenu CloneMenu(VRCExpressionsMenu menu, Dictionary<Object,Object> map, HashSet<VRCExpressionsMenu> visited)
            {
                if(!menu || visited.Contains(menu)) return menu;
                visited.Add(menu);
                menu = (VRCExpressionsMenu)CloneObject(menu, ctx, map);
                foreach(var control in menu.controls)
                {
                    if(control.type != ControlType.SubMenu) continue;
                    control.subMenu = CloneMenu(control.subMenu, map, visited);
                }
                return menu;
            }

            private static void CombineSubMenu(VRCExpressionsMenu menu, HashSet<VRCExpressionsMenu> visited)
            {
                // 同名のSubMenuを統合
                if(!menu || visited.Contains(menu)) return;
                visited.Add(menu);
                var firsts = new Dictionary<string, VRCExpressionsMenu>();
                for(int i = 0; i < menu.controls.Count; i++)
                {
                    var control = menu.controls[i];
                    if(control.type != ControlType.SubMenu || !control.subMenu) continue;
                    if(!firsts.ContainsKey(control.name))
                    {
                        firsts[control.name] = control.subMenu;
                        continue;
                    }
                    firsts[control.name].controls.AddRange(control.subMenu.controls);
                    menu.controls.RemoveAt(i);
                    i--;
                }

                foreach(var child in menu.controls)
                    if(child.subMenu) CombineSubMenu(child.subMenu, visited);
            }

            private static void ResolveOver(VRCExpressionsMenu menu, HashSet<VRCExpressionsMenu> visited)
            {
                // メニュー項目がオーバーしてる場合は子メニューを作成してそこにオーバー分を入れる
                if(!menu || visited.Contains(menu)) return;
                visited.Add(menu);
                if(menu.controls.Count > VRCExpressionsMenu.MAX_CONTROLS)
                {
                    int last = VRCExpressionsMenu.MAX_CONTROLS - 1;
                    int count = menu.controls.Count - last;
                    var menuNext = CreateMenu($"{menu.name}_Next");
                    menuNext.controls = menu.controls.GetRange(last, count);
                    menu.controls.RemoveRange(last, count);
                    AddMenu(menu, menuNext, iconNext, "Next");
                }

                foreach(var child in menu.controls)
                    if(child.subMenu) ResolveOver(child.subMenu, visited);

                static void AddMenu(VRCExpressionsMenu menu, VRCExpressionsMenu menu2, Texture2D icon = null, string name = null)
                {
                    menu.controls.Add(
                        new Control
                        {
                            icon = icon,
                            name = name ?? menu2.name,
                            subMenu = menu2,
                            type = ControlType.SubMenu
                        }
                    );
                }
            }

            private static VRCExpressionsMenu InternalToExpressions(InternalMenu internalMenu, VRCExpressionsMenu parent)
            {
                var control = new Control{icon = internalMenu.icon, name = internalMenu.name, value = internalMenu.value, type = ToControlType(internalMenu.type)};
                switch(internalMenu.type)
                {
                    case InternalMenuType.Folder:
                        control.subMenu = CreateMenu(internalMenu.name);
                        foreach(var menu in internalMenu.menus) InternalToExpressions(menu, control.subMenu);
                        break;
                    case InternalMenuType.Toggle:
                    case InternalMenuType.Trigger:
                        control.parameter = new Parameter{name = internalMenu.parameterName};
                        break;
                    case InternalMenuType.Slider:
                        control.subParameters = new[]{new Parameter{name = internalMenu.parameterName}};
                        break;
                }
                if(!parent) parent = CreateMenu();
                parent.controls.Add(control);
                return parent;

                static ControlType ToControlType(InternalMenuType type)
                {
                    switch(type)
                    {
                        case InternalMenuType.Folder: return ControlType.SubMenu;
                        case InternalMenuType.Toggle: return ControlType.Toggle;
                        case InternalMenuType.Slider: return ControlType.RadialPuppet;
                        case InternalMenuType.Trigger: return ControlType.Button;
                        default: throw new Exception($"Unknown type {type}");
                    }
                }
            }

            private static VRCExpressionsMenu CreateMenu(string name = ConstantValues.TOOL_NAME)
            {
                var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                menu.name = name;
                AssetDatabase.AddObjectToAsset(menu, ctx.AssetContainer);
                return menu;
            }

            private static VRCExpressionParameters InternalToExpressionParameters(List<InternalParameter> internalParameters, VRCExpressionParameters parameters)
            {
                if(!parameters)
                {
                    parameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                    parameters.name = ConstantValues.TOOL_NAME + "Parameters";
                    parameters.parameters = new VRCExpressionParameters.Parameter[0];
                    AssetDatabase.AddObjectToAsset(parameters, ctx.AssetContainer);
                }

                foreach(var param in internalParameters)
                    AddParameter(parameters, param.name, param.isLocalOnly, param.isSave, param.defaultValue, ToValueType(param.type));

                return parameters;

                static ValueType ToValueType(InternalParameterType type)
                {
                    switch(type)
                    {
                        case InternalParameterType.Bool: return ValueType.Bool;
                        case InternalParameterType.Int: return ValueType.Int;
                        case InternalParameterType.Float: return ValueType.Float;
                        default: throw new Exception($"Unknown type {type}");
                    }
                }

                static void AddParameter(VRCExpressionParameters parameters, string name, bool isLocalOnly, bool isSave, float defaultValue, VRCExpressionParameters.ValueType type)
                {
                    if(parameters.parameters.Any(p => p.name == name)) return;

                    Array.Resize(ref parameters.parameters, parameters.parameters.Length+1);
                    parameters.parameters[parameters.parameters.Length-1] = new VRCExpressionParameters.Parameter
                    {
                        defaultValue = defaultValue,
                        name = name,
                        networkSynced = !isLocalOnly,
                        saved = isSave,
                        valueType = type
                    };
                }
            }
        }
    }
    #endif
}
