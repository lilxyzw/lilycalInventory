#if LIL_VRCSDK3A
using System;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor;
using jp.lilxyzw.avatarmodifier.runtime;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.avatarmodifier
{
    internal static class VRChatHelper
    {
        internal static AnimatorController TryGetFXAnimatorController(this VRCAvatarDescriptor descriptor, BuildContext ctx)
        {
            AnimatorController CreateFXController()
            {
                var controller = new AnimatorController{name = "FX"};
                AssetDatabase.AddObjectToAsset(controller, ctx.AssetContainer);
                return controller;
            }
            if(!descriptor.customizeAnimationLayers)
            {
                descriptor.customizeAnimationLayers = true;
            }
            for(int i = 0; i < descriptor.baseAnimationLayers.Length; i++)
            {
                if(descriptor.baseAnimationLayers[i].type != VRCAvatarDescriptor.AnimLayerType.FX) continue;
                var layer = descriptor.baseAnimationLayers[i];
                if(layer.isDefault)
                {
                    layer.isDefault = false;
                    layer.animatorController = null;
                }
                if(layer.animatorController) return (AnimatorController)layer.animatorController;
                var controllerI = CreateFXController();
                layer.animatorController = controllerI;
                descriptor.baseAnimationLayers[i] = layer;
                return controllerI;
            }
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

        internal static void MergeParameters(this VRCAvatarDescriptor descriptor, VRCExpressionsMenu menu, VRCExpressionParameters parameters)
        {
            if(!descriptor.customExpressions)
            {
                descriptor.customExpressions = true;
                descriptor.expressionsMenu = null;
                descriptor.expressionParameters = null;
            }
            if(descriptor.expressionsMenu) descriptor.expressionsMenu.Merge(menu);
            else descriptor.expressionsMenu = menu;
            if(descriptor.expressionParameters) descriptor.expressionParameters.Merge(parameters);
            else descriptor.expressionParameters = parameters;
        }

        internal static VRCExpressionsMenu CreateMenu(BuildContext ctx)
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            menu.name = "AvatarModifier";
            AssetDatabase.AddObjectToAsset(menu, ctx.AssetContainer);
            return menu;
        }

        internal static VRCExpressionParameters CreateParameters(BuildContext ctx)
        {
            var parameter = ScriptableObject.CreateInstance<VRCExpressionParameters>();
            parameter.name = "AvatarModifierParameters";
            parameter.parameters = new VRCExpressionParameters.Parameter[0];
            AssetDatabase.AddObjectToAsset(parameter, ctx.AssetContainer);
            return parameter;
        }

        internal static void Merge(this VRCExpressionsMenu menu, VRCExpressionsMenu menu2)
        {
            if(menu.controls.Count >= VRCExpressionsMenu.MAX_CONTROLS)
                throw new Exception("Menu Over!!!!!!!");
            if(menu.controls.Count + menu2.controls.Count <= VRCExpressionsMenu.MAX_CONTROLS)
                menu.controls.AddRange(menu2.controls);
            else
            {
                menu.controls.Add(
                    new VRCExpressionsMenu.Control
                    {
                        name = menu2.name,
                        subMenu = menu2,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu
                    }
                );
                return;
            }
        }

        internal static void Merge(this VRCExpressionParameters parameters, VRCExpressionParameters parameters2)
        {
            if(parameters.CalcTotalCost() + parameters2.CalcTotalCost() >= VRCExpressionParameters.MAX_PARAMETER_COST)
                throw new Exception("Parameter Over!!!!!!!");

            var size = parameters.parameters.Length;
            Array.Resize(ref parameters.parameters, parameters.parameters.Length + parameters2.parameters.Length);
            for(int i = 0; i < parameters2.parameters.Length; i++)
            {
                parameters.parameters[size+i] = parameters2.parameters[i];
            }
        }

        internal static void AddMenu(this VRCExpressionsMenu menu, VRCExpressionsMenu menu2)
        {
            if(menu.controls.Count >= VRCExpressionsMenu.MAX_CONTROLS)
                throw new Exception("Menu Over!!!!!!!");

            menu.controls.Add(
                new VRCExpressionsMenu.Control
                {
                    name = menu2.name,
                    subMenu = menu2,
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu
                }
            );
        }

        internal static void AddParameter(this VRCExpressionParameters parameters, string name, bool isLocalOnly, bool isSave, float defaultValue, VRCExpressionParameters.ValueType type)
        {
            if(!parameters.parameters.Any(p => p.name == name))
            {
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

        internal static void AddParameterToggle(this VRCExpressionParameters parameters, string name, bool isLocalOnly, bool isSave, float defaultValue = 0)
        {
            parameters.AddParameter(name, isLocalOnly, isSave, defaultValue, VRCExpressionParameters.ValueType.Bool);
        }

        internal static void AddParameterInt(this VRCExpressionParameters parameters, string name, bool isLocalOnly, bool isSave, float defaultValue = 0)
        {
            parameters.AddParameter(name, isLocalOnly, isSave, defaultValue, VRCExpressionParameters.ValueType.Int);
        }

        internal static void AddParameterFloat(this VRCExpressionParameters parameters, string name, bool isLocalOnly, bool isSave, float defaultValue = 0)
        {
            parameters.AddParameter(name, isLocalOnly, isSave, defaultValue, VRCExpressionParameters.ValueType.Float);
        }

        internal static VRCExpressionsMenu.Control GetMenuControlToggle(this MenuBaseComponent component, float value = 1)
        {
            var name = component.menuName;
            return new VRCExpressionsMenu.Control
            {
                icon = component.icon,
                name = name,
                parameter = new VRCExpressionsMenu.Control.Parameter{name = name},
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                value = value
            };
        }

        internal static VRCExpressionsMenu.Control GetMenuControlRadialPuppet(this MenuBaseComponent component, float value = 1)
        {
            var name = component.menuName;
            return new VRCExpressionsMenu.Control
            {
                icon = component.icon,
                name = name,
                subParameters = new[]{new VRCExpressionsMenu.Control.Parameter{name = name}},
                type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                value = value
            };
        }
    }
}
#endif
