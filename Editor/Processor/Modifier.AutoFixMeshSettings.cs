using System.Linq;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ApplyMeshSettingsModifier(GameObject root, AutoFixMeshSettings[] settingss)
            {
                if(settingss == null || settingss.Length == 0) return;
                if(settingss.Length > 1)
                {
                    ErrorHelper.Report("dialog.error.autoFixMeshSettingsDuplication", settingss);
                    return;
                }
                var settings = settingss[0];

                root.transform.GetPositionAndRotation(out var position, out var rotation);
                root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                var renderers = root.GetComponentsInChildren<Renderer>(true).Where(r => !settings.ignoreRenderers.Contains(r)).ToArray();
                var probeAnchor = settings.meshSettings.anchorOverride ? settings.meshSettings.anchorOverride : GetHumanBone(root, HumanBodyBones.Chest);
                var rootBone = settings.meshSettings.rootBone ? settings.meshSettings.rootBone : GetHumanBone(root, HumanBodyBones.Hips);
                var bounds = settings.meshSettings.autoCalculateBounds ? SumBounds(renderers.Where(r => !(r is ParticleSystemRenderer)).ToArray()) : settings.meshSettings.bounds;
                if(settings.meshSettings.autoCalculateBounds) bounds.center -= rootBone.position;
                foreach(var renderer in renderers)
                {
                    renderer.shadowCastingMode = settings.meshSettings.castShadows;
                    renderer.receiveShadows = settings.meshSettings.receiveShadows;
                    renderer.lightProbeUsage = settings.meshSettings.lightProbes;
                    renderer.reflectionProbeUsage = settings.meshSettings.reflectionProbes;
                    renderer.probeAnchor = probeAnchor;
                    renderer.motionVectorGenerationMode = settings.meshSettings.motionVectors;
                    renderer.allowOcclusionWhenDynamic = settings.meshSettings.dynamicOcclusion;

                    if(renderer is SkinnedMeshRenderer s)
                    {
                        s.updateWhenOffscreen = settings.meshSettings.updateWhenOffscreen;
                        s.skinnedMotionVectors = settings.meshSettings.skinnedMotionVectors;
                        if(!s.gameObject.GetComponent<Cloth>())
                        {
                            s.rootBone = rootBone;
                            s.localBounds = bounds;
                        }
                    }
                }
                root.transform.SetPositionAndRotation(position, rotation);
            }

            private static Transform GetHumanBone(GameObject root, HumanBodyBones humanBodyBones)
            {
                var animator = root.GetComponent<Animator>();
                if(animator)
                {
                    if(animator && animator.avatar && animator.avatar.isHuman)
                    {
                        var bone = animator.GetBoneTransform(humanBodyBones);
                        if(bone) return bone;
                    }
                    return animator.avatarRoot;
                }
                return root.transform;
            }

            private static Bounds SumBounds(Renderer[] renderers)
            {
                var bounds = new Bounds();
                foreach(var renderer in renderers)
                {
                    if(renderer is MeshRenderer mr)
                    {
                        bounds.Encapsulate(mr.bounds);
                    }
                    else if(renderer is SkinnedMeshRenderer smr && smr.sharedMesh)
                    {
                        smr.transform.GetPositionAndRotation(out var position, out var rotation);
                        smr.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                        var bakedMesh = new Mesh();
                        smr.BakeMesh(bakedMesh);
                        var bakedBounds = bakedMesh.bounds;
                        if(bounds.extents.magnitude == 0) bounds = bakedBounds;
                        else bounds.Encapsulate(bakedBounds);
                        smr.transform.SetPositionAndRotation(position, rotation);
                    }
                }
                return bounds;
            }
        }
    }
}
