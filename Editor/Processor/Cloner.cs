using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    internal class Cloner
    {
        internal static Dictionary<Material,Material> materialMap;
        internal static void DeepCloneAssets(BuildContext context)
        {
            CloneAnimatorControllers(context);
            AnimationUtil.CloneAllControllers(context);
            CloneAssetForPlatform(context);
        }

        private static void CloneAnimatorControllers(BuildContext context)
        {
            var animators = context.AvatarRootObject.GetComponentsInChildren<Animator>(true);
            if(animators.Length == 0) return;

            for(int i = 0; i < animators.Length; i++)
            {
                var animator = animators[i];
                if(animator.runtimeAnimatorController && !context.IsTemporaryAsset(animator.runtimeAnimatorController))
                    animator.runtimeAnimatorController = AnimationUtil.DeepCloneAnimator(context, animator.runtimeAnimatorController);
            }
        }

        internal static Material[] CloneAllMaterials(BuildContext context)
        {
            CloneRendererMaterials(context);
            CloneAnimationClipMaterials(context);

            return materialMap.Select(m => m.Value).Distinct().Where(m => m).ToArray();
        }

        private static void CloneRendererMaterials(BuildContext context)
        {
            var renderers = context.AvatarRootObject.GetComponentsInChildren<Renderer>(true);
            for(int i = 0; i < renderers.Length; i++)
            {
                var sharedMaterials = renderers[i].sharedMaterials;
                for(int j = 0; j < sharedMaterials.Length; j++)
                    sharedMaterials[j] = CloneMaterial(sharedMaterials[j], context);

                renderers[i].sharedMaterials = sharedMaterials;
            }
        }

        private static void CloneAnimationClipMaterials(BuildContext context)
        {
            var controllers = new HashSet<RuntimeAnimatorController>();
            controllers.UnionWith(context.AvatarRootObject.GetComponentsInChildren<Animator>(true).Select(a => a.runtimeAnimatorController));

            #if LIL_VRCSDK3A
            var descriptor = context.AvatarDescriptor;
            controllers.UnionWith(descriptor.specialAnimationLayers.Select(a => a.animatorController));
            if(descriptor.customizeAnimationLayers)
                controllers.UnionWith(descriptor.baseAnimationLayers.Select(a => a.animatorController));
            #endif

            controllers.RemoveWhere(c => !c);

            var clips = controllers.SelectMany(c => c.animationClips).Where(c => c).ToArray();


            for(int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                var frames = new ObjectReferenceKeyframe[bindings.Length][];

                for(int j = 0; j < bindings.Length; j++)
                    frames[j] = AnimationUtility.GetObjectReferenceCurve(clip, bindings[j]);

                if(!frames.SelectMany(f => f).Any(f => f.value is Material)) continue;
                if(!context.IsTemporaryAsset(clip))
                {
                    Debug.Log($"Can't Modify {clip.name}");
                    continue;
                }

                for(int j = 0; j < frames.Length; j++)
                    for(int k = 0; k < frames[j].Length; k++)
                        if(frames[j][k].value is Material m) frames[j][k].value = CloneMaterial(m, context);

                #if UNITY_2020_1_OR_NEWER
                AnimationUtility.SetObjectReferenceCurves(clip, bindings, frames);
                #else
                for(int j = 0; j < bindings.Length; j++)
                    AnimationUtility.SetObjectReferenceCurve(clip, bindings[j], frames[j]);
                #endif
            }

            var renderers = context.AvatarRootObject.GetComponentsInChildren<Renderer>(true);
            for(int i = 0; i < renderers.Length; i++)
            {
                var sharedMaterials = renderers[i].sharedMaterials;
                for(int j = 0; j < sharedMaterials.Length; j++)
                    sharedMaterials[j] = CloneMaterial(sharedMaterials[j], context);

                renderers[i].sharedMaterials = sharedMaterials;
            }
        }

        private static Material CloneMaterial(Material material, BuildContext context)
        {
            if(!material) return material;
            if(materialMap.ContainsKey(material)) return materialMap[material];
            if(context.IsTemporaryAsset(material)) return materialMap[material] = material;
            var clone = Object.Instantiate(material); // new Material(material) is slow
            #if LIL_NDMF
            ObjectRegistry.RegisterReplacedObject(material, clone);
            #endif
            AssetDatabase.AddObjectToAsset(clone, context.AssetContainer);
            return materialMap[material] = clone;
        }

        private static Object CloneObject(Object obj, BuildContext context, Dictionary<Object,Object> map)
        {
            if(!obj || context.IsTemporaryAsset(obj)) return obj;
            if(map.ContainsKey(obj)) return map[obj];
            var clone = Object.Instantiate(obj);
            #if LIL_NDMF
            ObjectRegistry.RegisterReplacedObject(obj, clone);
            #endif
            AssetDatabase.AddObjectToAsset(clone, context.AssetContainer);
            return map[obj] = clone;
        }

        private static void CloneAssetForPlatform(BuildContext context)
        {
            #if LIL_VRCSDK3A
                var map = new Dictionary<Object,Object>();
                context.AvatarDescriptor.expressionParameters = (VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters)CloneObject(context.AvatarDescriptor.expressionParameters, context, map);
                context.AvatarDescriptor.expressionsMenu = (VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu)CloneObject(context.AvatarDescriptor.expressionsMenu, context, map);
            #endif
        }
    }
}
