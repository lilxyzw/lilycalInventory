#if !LIL_NDMF && LIL_VRCSDK3A
using System;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.lilxyzw.avatarmodifier.vrchat
{
    public class AvatarPreprocessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -1100;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try
            {
                var ctx = new BuildContext(avatarGameObject);

                Processor.FindComponent(ctx);
                Processor.Clone(ctx);
                Processor.Modify(ctx);
                Processor.RemoveComponent(ctx);
                Processor.Optimize(ctx);

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
