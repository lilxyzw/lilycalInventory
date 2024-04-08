using System.IO;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal class ToolSettings
    {
        private const string MENU_DIRECT_BLENDTREE = "Tools/lilycalInventory/Use Direct Blend Tree";
        private static readonly string PATH_PREF = $"{UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder}/jp.lilxyzw";
        private static readonly string FILENAME_SETTING = ConstantValues.PACKAGE_NAME + ".json";
        private static string PATH_SETTING => $"{PATH_PREF}/{FILENAME_SETTING}";
        private static ToolSettings m_Instance;
        internal static ToolSettings instance => m_Instance != null ? m_Instance : m_Instance = Load();

        // 設定は現状DirectBlendTreeを使うかのみ
        public bool useDirectBlendTree = true;

        [MenuItem(MENU_DIRECT_BLENDTREE)]
        private static void ToggleDirectBlendTree()
        {
            instance.useDirectBlendTree = !instance.useDirectBlendTree;
            Save();
        }

        [MenuItem(MENU_DIRECT_BLENDTREE, true)]
        private static bool ToggleDirectBlendTreeValidate()
        {
            Menu.SetChecked(MENU_DIRECT_BLENDTREE, instance.useDirectBlendTree);
            return true;
        }

        private static ToolSettings Load()
        {
            if(!Directory.Exists(PATH_PREF)) Directory.CreateDirectory(PATH_PREF);
            if(!File.Exists(PATH_SETTING)) File.WriteAllText(PATH_SETTING, JsonUtility.ToJson(new ToolSettings(), true));
            return JsonUtility.FromJson<ToolSettings>(SafeIO.LoadFile(PATH_SETTING));
        }

        private static void Save()
        {
            if(!Directory.Exists(PATH_PREF)) Directory.CreateDirectory(PATH_PREF);
            SafeIO.SaveFile(PATH_SETTING, JsonUtility.ToJson(instance, true));
        }
    }
}
