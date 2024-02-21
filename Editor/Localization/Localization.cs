using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Globalization;
using UnityEngine;
using System.Text;

namespace jp.lilxyzw.materialmodifier
{
    using runtime;

    internal class Localization
    {
        private static readonly string PATH_PREF = $"{UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder}/jp.lilxyzw";
        private static readonly string FILENAME_SETTING = "materialmodifier.language.conf";
        private static string PATH_SETTING => $"{PATH_PREF}/{FILENAME_SETTING}";
        private static List<Dictionary<string, string>> languages = new List<Dictionary<string, string>>();
        private static List<string> codes = new List<string>();
        private static string[] names;
        private static int number;

        [InitializeOnLoadMethod]
        private static void LoadDatas()
        {
            var paths = Directory.GetFiles(AssetDatabase.GUIDToAssetPath("576877b5c458c4a4f922bde6f9a89d44"), "*.json");
            var tmpNames = new List<string>();
            foreach(var path in paths)
            {
                var langData = File.ReadAllText(path);
                var lang = JsonDictionaryParser.Deserialize<string>(langData);
                if(lang == null) continue;
                var code = Path.GetFileNameWithoutExtension(path).ToLower();
                languages.Add(lang);
                codes.Add(code);
                try
                {
                    var cul = new CultureInfo(code);
                    tmpNames.Add(cul.NativeName);
                }
                catch
                {
                    tmpNames.Add(code);
                }
            }
            names = tmpNames.ToArray();
            number = codes.IndexOf(LoadLanguageSettings());
            if(number == -1) number = codes.IndexOf("en-us");
            if(number == -1) number = 0;
        }

        internal static string S(string key)
        {
            return languages[number].TryGetValue(key, out string o) ? o : null;
        }

        internal static GUIContent G(string key)
        {
            return new GUIContent(S(key) ?? key, S($"{key}.tooltip"));
        }

        internal static GUIContent G(SerializedProperty property)
        {
            return G($"inspector.{property.name}");
        }

        internal static void SelectLanguageGUI()
        {
            EditorGUI.BeginChangeCheck();
            number = EditorGUILayout.Popup("Editor Language", number, names);
            if(EditorGUI.EndChangeCheck()) SaveLanguageSettings();
        }

        internal static void SelectLanguageGUI(Rect position)
        {
            EditorGUI.BeginChangeCheck();
            number = EditorGUI.Popup(position, "Editor Language", number, names);
            if(EditorGUI.EndChangeCheck()) SaveLanguageSettings();
        }

        private static string LoadLanguageSettings()
        {
            if(!Directory.Exists(PATH_PREF)) Directory.CreateDirectory(PATH_PREF);
            if(!File.Exists(PATH_SETTING)) SafeIO.SaveFile(PATH_SETTING, CultureInfo.CurrentCulture.Name.ToLower());
            return SafeIO.LoadFile(PATH_SETTING);
        }

        private static void SaveLanguageSettings()
        {
            if(!Directory.Exists(PATH_PREF)) Directory.CreateDirectory(PATH_PREF);
            SafeIO.SaveFile(PATH_SETTING, codes[number]);
        }
    }

    internal class SafeIO
    {
        internal static void SaveFile(string path, string content)
        {
            using(var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            using(var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.Write(content);
            }
        }

        internal static string LoadFile(string path)
        {
            using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using(var sr = new StreamReader(fs, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
    }

    [CustomPropertyDrawer(typeof(LILLocalizeAttribute))]
    internal class LILLocalizeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LILLocalizeAttribute loc = attribute as LILLocalizeAttribute;
            if(loc.name == null)
                EditorGUI.PropertyField(position, property, Localization.G(property));
            else
                EditorGUI.PropertyField(position, property, Localization.G(loc.name));
        }
    }
}
