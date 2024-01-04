#if !LIL_NDMF
using System;
using System.Linq;
using jp.lilxyzw.materialmodifier.runtime;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using Object = UnityEngine.Object;

namespace jp.lilxyzw.materialmodifier.vrchat
{
    public class AvatarPreprocessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -1100;
        private static MaterialModifier[] modifiers;
        private static MaterialOptimizer[] optimizers;
        private static Material[] materials;
        private static bool shouldModify;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try
            {
                var ctx = new BuildContext(avatarGameObject);

                var components = ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>();
                modifiers = components.Select(c => c as MaterialModifier).Where(c => c != null).ToArray();
                optimizers = components.Select(c => c as MaterialOptimizer).Where(c => c != null).ToArray();
                shouldModify = components.Length != 0;
                foreach(var component in ctx.AvatarRootObject.GetComponentsInChildren<AvatarTagComponent>(true))
                    Object.DestroyImmediate(component);

                if(shouldModify) materials = Cloner.DeepCloneMaterials(ctx);
                if(shouldModify && modifiers.Length != 0) Modifier.ModifyMaterials(materials, modifiers);
                if(shouldModify && optimizers.Length != 0) Optimizer.OptimizeMaterials(materials);

                AssetDatabase.SaveAssets();
                return true;
            }
            catch(Exception e)
            {
                EditorUtility.DisplayDialog("lilMaterialModifier", $"{Localization.S("dialog.error.processfailed")}\r\n{e}", "OK");
                return false;
            }
        }
    }

    public class VRChatProcessor : IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => 0;

        public void OnPostprocessAvatar()
        {
            try
            {
                BuildContext.CreanAssets();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif
