#if !LIL_NDMF && LIL_VRCSDK3A
using System;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.lilxyzw.lilycalinventory.vrchat
{
    using runtime;

    // NDMFがない場合はこちらで処理
    public class AvatarPreprocessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -11000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            return OnPreprocessAvatarInternal(avatarGameObject);
        }

        [MenuItem("Tools/lilycalInventory/Test in Editor")]
        private static void TestBuild()
        {
            if(!EditorApplication.isPlaying || !Selection.activeGameObject || !Selection.activeGameObject.GetComponent<VRCAvatarDescriptor>())
            {
                EditorUtility.DisplayDialog("lilycalIntentory", Localization.S("dialog.error.testInEditor"), "OK");
                return;
            }

            OnPreprocessAvatarInternal(Selection.activeGameObject);
        }

        private static bool OnPreprocessAvatarInternal(GameObject avatarGameObject)
        {
            try
            {
                var ctx = new BuildContext(avatarGameObject);

                Processor.FindComponent(ctx);
                Processor.ModifyPreProcess(ctx);
                Processor.ModifyPostProcess(ctx);
                Processor.RemoveComponent(ctx);
                Processor.Optimize(ctx); // NDMFでは-1025で実行されるけど処理内容が単純なのでここで処理

                AssetDatabase.SaveAssets();
                return true;
            }
            catch(Exception e)
            {
                EditorUtility.DisplayDialog(ConstantValues.TOOL_NAME, $"{Localization.S("dialog.error.processfailed")}\r\n{e}", "OK");
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
