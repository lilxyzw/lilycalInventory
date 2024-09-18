#if LIL_NDMF
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    [ParameterProviderFor(typeof(AutoDresser))]
    internal class AutoDresserParameterProvider : IParameterProvider
    {
        private readonly AutoDresser component;
        public AutoDresserParameterProvider(AutoDresser component) => this.component = component;
        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) => LIParameterProviderUtils.CreateProvidedParameter(component);
        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null){}
    }

    [ParameterProviderFor(typeof(CostumeChanger))]
    internal class CostumeChangerParameterProvider : IParameterProvider
    {
        private readonly CostumeChanger component;
        public CostumeChangerParameterProvider(CostumeChanger component) => this.component = component;
        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null){}
        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
        {
            if(!component.IsEnabledInBuild()) return new ProvidedParameter[0];
            if(!component.isLocalOnly) return LIParameterProviderUtils.CreateProvidedParameter(component, AnimatorControllerParameterType.Int, component.isLocalOnly);
            return LIParameterProviderUtils.CreateProvidedParameterCompressedInt(component, component.GetMenuName(), component.costumes.Length);
        }
    }

    [ParameterProviderFor(typeof(ItemToggler))]
    internal class ItemTogglerParameterProvider : IParameterProvider
    {
        private readonly ItemToggler component;
        public ItemTogglerParameterProvider(ItemToggler component) => this.component = component;
        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) => LIParameterProviderUtils.CreateProvidedParameter(component, AnimatorControllerParameterType.Bool, component.isLocalOnly);
        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null){}
    }

    [ParameterProviderFor(typeof(Prop))]
    internal class LIParameterProvider : IParameterProvider
    {
        private readonly Prop component;
        public LIParameterProvider(Prop component) => this.component = component;
        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) => LIParameterProviderUtils.CreateProvidedParameter(component);
        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null){}
    }

    [ParameterProviderFor(typeof(SmoothChanger))]
    internal class SmoothChangerParameterProvider : IParameterProvider
    {
        private readonly SmoothChanger component;
        public SmoothChangerParameterProvider(SmoothChanger component) => this.component = component;
        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null) => LIParameterProviderUtils.CreateProvidedParameter(component, AnimatorControllerParameterType.Float, component.isLocalOnly);
        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null){}
    }

    internal static class LIParameterProviderUtils
    {
        internal static IEnumerable<ProvidedParameter> CreateProvidedParameter(AutoDresser component)
        {
            if(!component.IsEnabledInBuild()) return new ProvidedParameter[0];
            var root = component.gameObject.GetAvatarRoot();
            if(!root) return new ProvidedParameter[0];

            var costumeCount = root.GetComponentsInChildren<AutoDresser>(true).Count(c => c.IsEnabledInBuild());
            return CreateProvidedParameterCompressedInt(component, "AutoDresser", costumeCount);
        }

        internal static IEnumerable<ProvidedParameter> CreateProvidedParameterCompressedInt(Component component, string name, int costumeCount)
        {
            var list = new List<ProvidedParameter>();
            list.Add(new ProvidedParameter(
                    name,
                    ParameterNamespace.Animator,
                    component,
                    LilycalInventoryPlugin.Instance,
                    AnimatorControllerParameterType.Int
                ){
                    IsAnimatorOnly = true,
                    IsHidden = false,
                    WantSynced = false
                });
            list.Add(new ProvidedParameter(
                    name+"_Local",
                    ParameterNamespace.Animator,
                    component,
                    LilycalInventoryPlugin.Instance,
                    AnimatorControllerParameterType.Int
                ){
                    IsAnimatorOnly = false,
                    IsHidden = false,
                    WantSynced = false
                });
            var bits = ObjHelper.ToNBitInt(costumeCount);
            for(int bit = 0; bit < bits; bit++)
            {
                list.Add(new ProvidedParameter(
                        $"{name}_Bool{bit}",
                        ParameterNamespace.Animator,
                        component,
                        LilycalInventoryPlugin.Instance,
                        AnimatorControllerParameterType.Bool
                    ){
                        IsAnimatorOnly = false,
                        IsHidden = false,
                        WantSynced = true
                    });
            }
            return list;
        }

        internal static ProvidedParameter[] CreateProvidedParameter(Prop component)
        {
            if(!component.IsEnabledInBuild()) return new ProvidedParameter[0];
            return new ProvidedParameter[]{
                new(
                    component.GetMenuName(),
                    ParameterNamespace.Animator,
                    component,
                    LilycalInventoryPlugin.Instance,
                    AnimatorControllerParameterType.Bool
                ){
                    IsAnimatorOnly = false,
                    IsHidden = false,
                    WantSynced = !component.isLocalOnly
                }};
        }

        internal static ProvidedParameter[] CreateProvidedParameter(MenuBaseComponent component, AnimatorControllerParameterType type, bool isLocalOnly = false)
        {
            if(!component.IsEnabledInBuild()) return new ProvidedParameter[0];
            return new ProvidedParameter[]{
                new(
                    component.GetMenuName(),
                    ParameterNamespace.Animator,
                    component,
                    LilycalInventoryPlugin.Instance,
                    type
                ){
                    IsAnimatorOnly = false,
                    IsHidden = false,
                    WantSynced = !isLocalOnly
                }};
        }
    }
}
#endif
