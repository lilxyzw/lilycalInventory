using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
{
    using runtime;

    internal class Localization
    {
        private static readonly string PATH_PREF = $"{UnityEditorInternal.InternalEditorUtility.unityPreferencesFolder}/jp.lilxyzw";
        private static readonly string FILENAME_SETTING = "avatarmodifier.language.conf";
        private static string PATH_SETTING => $"{PATH_PREF}/{FILENAME_SETTING}";
        private static List<Dictionary<string, string>> languages = new List<Dictionary<string, string>>();
        private static List<string> codes = new List<string>();
        private static string[] names;
        private static int number;

        [InitializeOnLoadMethod]
        internal static void LoadDatas()
        {
            var paths = Directory.GetFiles(AssetDatabase.GUIDToAssetPath("0c7d604810e251042910620b6b1d9a59"), "*.json");
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
            number = GetIndexByCode(LoadLanguageSettings());
        }

        internal static string[] GetCodes()
        {
            return codes.ToArray();
        }

        private static int GetIndexByCode(string code)
        {
            var index = codes.IndexOf(code);
            if(index == -1) index = codes.IndexOf("en-us");
            if(index == -1) number = 0;
            return index;
        }

        private static string S(string key, int index)
        {
            return languages[index].TryGetValue(key, out string o) ? o : null;
        }

        internal static string S(string key, string code)
        {
            return S(key, GetIndexByCode(code));
        }

        internal static string S(string key)
        {
            return S(key, number);
        }

        internal static string SorKey(string key)
        {
            return languages[number].TryGetValue(key, out string o) ? o : key;
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
            if(!File.Exists(PATH_SETTING)) File.WriteAllText(PATH_SETTING, CultureInfo.CurrentCulture.Name.ToLower());
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
