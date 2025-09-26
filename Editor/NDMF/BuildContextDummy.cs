#if !LIL_NDMF
using System.IO;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    // NDMFがない場合のダミー
    internal class BuildContext
    {
        internal GameObject AvatarRootObject;
        internal Object AssetContainer;

        private const string PATH_ROOT = "Assets";
        private const string PATH_GEN = "__Generated_" + ConstantValues.TOOL_NAME;

        internal BuildContext(GameObject gameObject)
        {
            AvatarRootObject = gameObject;
            AssetContainer = ScriptableObject.CreateInstance<AssetContainer>();

            if(!Directory.Exists($"{PATH_ROOT}/{PATH_GEN}"))
                AssetDatabase.CreateFolder(PATH_ROOT, PATH_GEN);
            var path = $"{PATH_ROOT}/{PATH_GEN}/{gameObject.name}_{System.Guid.NewGuid()}.asset";
            AssetDatabase.CreateAsset(AssetContainer, path);
        }

        internal bool IsTemporaryAsset(Object obj)
        {
            return !EditorUtility.IsPersistent(obj) || AssetDatabase.GetAssetPath(obj) == AssetDatabase.GetAssetPath(AssetContainer);
        }

        internal static void CreanAssets()
        {
            AssetDatabase.DeleteAsset($"{PATH_ROOT}/{PATH_GEN}");
        }
    }
}
#endif
