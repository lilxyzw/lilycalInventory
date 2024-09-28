using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    internal class Cloner
    {
        internal static Dictionary<Material,Material> materialMap;

        // 汎用クローン
        internal static Object CloneObject(Object obj, BuildContext ctx, Dictionary<Object,Object> map)
        {
            if(!obj || ctx.IsTemporaryAsset(obj)) return obj;
            if(map.ContainsKey(obj)) return map[obj];
            var clone = Object.Instantiate(obj);
            RegisterReplacedObject(obj, clone);
            AssetDatabase.AddObjectToAsset(clone, ctx.AssetContainer);
            return map[obj] = clone;
        }

        // Materialをクローン
        internal static Material[] CloneAllMaterials()
        {
            // クローンしつつAssetContainerに登録
            Material CloneMaterial(Material material)
            {
                if(!material) return material;
                if(materialMap.ContainsKey(material)) return materialMap[material];
                if(Processor.ctx.IsTemporaryAsset(material)) return materialMap[material] = material;
                var clone = Object.Instantiate(material); // new Material(material) is slow
                RegisterReplacedObject(material, clone);
                AssetDatabase.AddObjectToAsset(clone, Processor.ctx.AssetContainer);
                return materialMap[material] = clone;
            }

            // Rendererのマテリアルスロット内をクローン
            foreach(var renderer in Processor.ctx.AvatarRootObject.GetComponentsInChildren<Renderer>(true))
            {
                var sharedMaterials = renderer.sharedMaterials;
                for(int i = 0; i < sharedMaterials.Length; i++)
                    sharedMaterials[i] = CloneMaterial(sharedMaterials[i]);

                renderer.sharedMaterials = sharedMaterials;
            }

            // Animatorをクローン
            var controllers = new HashSet<RuntimeAnimatorController>();

            foreach(var animator in Processor.ctx.AvatarRootObject.GetComponentsInChildren<Animator>(true))
            {
                if(ContainsMaterialReference(animator.runtimeAnimatorController))
                {
                    animator.runtimeAnimatorController = AnimatorCombiner.DeepCloneAnimator(Processor.ctx, animator.runtimeAnimatorController);
                    controllers.Add(animator.runtimeAnimatorController);
                }
            }

            foreach(var component in Processor.ctx.AvatarRootObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if(NotContainsAnimatorController(component)) continue;

                using var so = new SerializedObject(component);
                using var iter = so.GetIterator();
                var enterChildren = true;
                while(iter.Next(enterChildren))
                {
                    enterChildren = iter.propertyType != SerializedPropertyType.String;
                    if(iter.propertyType == SerializedPropertyType.ObjectReference && iter.objectReferenceValue is RuntimeAnimatorController controller && ContainsMaterialReference(controller))
                    {
                        iter.objectReferenceValue = AnimatorCombiner.DeepCloneAnimator(Processor.ctx, controller);
                        controllers.Add(iter.objectReferenceValue as RuntimeAnimatorController);
                    }
                }
            }

            var clips = controllers.SelectMany(c => c.animationClips).Where(c => c).ToArray();

            for(int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                var frames = new ObjectReferenceKeyframe[bindings.Length][];

                for(int j = 0; j < bindings.Length; j++)
                    frames[j] = AnimationUtility.GetObjectReferenceCurve(clip, bindings[j]);

                if(!frames.SelectMany(f => f).Any(f => f.value is Material)) continue;
                if(!Processor.ctx.IsTemporaryAsset(clip))
                {
                    throw new Exception($"Can't Modify {clip.name}");
                }

                for(int j = 0; j < frames.Length; j++)
                    for(int k = 0; k < frames[j].Length; k++)
                        if(frames[j][k].value is Material m) frames[j][k].value = CloneMaterial(m);

                for(int j = 0; j < bindings.Length; j++)
                    AnimationUtility.SetObjectReferenceCurve(clip, bindings[j], frames[j]);
            }

            return materialMap.Select(m => m.Value).Distinct().Where(m => m).ToArray();
        }

        internal static void RegisterReplacedObject(Object origin, Object clone)
        {
            #if LIL_NDMF
            nadena.dev.ndmf.ObjectRegistry.RegisterReplacedObject(origin, clone);
            #endif
        }

        internal static bool OriginEquals(Material origin, Material clone)
        {
            #if LIL_NDMF
            return nadena.dev.ndmf.ObjectRegistry.GetReference(origin) == nadena.dev.ndmf.ObjectRegistry.GetReference(clone);
            #else
            return materialMap.ContainsKey(origin) && materialMap[origin] == clone;
            #endif
        }

        private static bool NotContainsAnimatorController(MonoBehaviour component)
        {
            return !component || component is runtime.AvatarTagComponent
            #if LIL_VRCSDK3A
                || component is VRC.Dynamics.VRCPhysBoneBase
                || component is VRC.Dynamics.VRCPhysBoneColliderBase
                || component is VRC.Dynamics.VRCConstraintBase
                || component is VRC.Dynamics.ContactBase
                || component is VRC.Core.PipelineManager
            #endif
            ;
        }

        private static bool ContainsMaterialReference(RuntimeAnimatorController controller)
        {
            if(!controller) return false;
            return controller.animationClips.Any(clip => AnimationUtility.GetObjectReferenceCurveBindings(clip).Any(b => AnimationUtility.GetObjectReferenceCurve(clip, b).Any(curve => curve.value is Material)));
        }
    }
}
