using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal class ChangeLogViewer : ScriptableSingleton<ChangeLogViewer>
    {
        internal string changelogEn;
        internal string changelogJp;

        internal static IEnumerator GetChangelogEn()
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(ConstantValues.URL_CHANGELOG_EN);
            yield return webRequest.SendWebRequest();
            if(webRequest.result != UnityWebRequest.Result.ConnectionError)
            {
                instance.changelogEn = ParseChangelog(webRequest.downloadHandler.text);
            }
        }

        internal static IEnumerator GetChangelogJp()
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(ConstantValues.URL_CHANGELOG_JP);
            yield return webRequest.SendWebRequest();
            if(webRequest.result != UnityWebRequest.Result.ConnectionError)
            {
                instance.changelogJp = ParseChangelog(webRequest.downloadHandler.text).Replace(" ", "\u00A0");
            }
        }

        private static string ParseChangelog(string md)
        {
            var sb = new StringBuilder();
            using var sr = new StringReader(md);
            bool isHeader = true;
            string line;
            while((line = sr.ReadLine()) != null)
            {
                if(isHeader)
                {
                    isHeader = !line.StartsWith("## [");
                    if(isHeader) continue;
                }
                line = ReplaceSyntax(line, "`", "\u2006<color=#e96900>", "</color>\u2006");
                if(line.StartsWith("### "))
                {
                    sb.AppendLine($"<size=15><b>{line.Substring(4)}</b></size>");
                }
                else if(line.StartsWith("## "))
                {
                    sb.AppendLine($"<color=#2d9c63><size=20><b>{line.Substring(3)}</b></size></color>");
                }
                else
                {
                    sb.AppendLine("  " + line);
                }
            }
            return sb.ToString();
        }

        private static string ReplaceSyntax(string s, string syntax, string start, string end)
        {
            while(true)
            {
                var first = s.IndexOf(syntax);
                if(first == -1) return s;

                var length = syntax.Length;
                var second = s.IndexOf(syntax, first + length);
                if(second == -1) return s;

                s = s.Remove(first) + start + s.Substring(first + length);
                var second2 = s.IndexOf(syntax);
                s = s.Remove(second2) + end + s.Substring(second2 + length);
            }
        }
    }

    internal class ChangeLogWindow : EditorWindow
    {
        private static GUIStyle style;
        private Vector2 scrollPosition = Vector2.zero;

        internal static void Init()
        {
            var window = (ChangeLogWindow)GetWindow(typeof(ChangeLogWindow));
            window.Show();
        }

        void OnGUI()
        {
            if(style == null) style = new GUIStyle(EditorStyles.label){richText = true, wordWrap = true};

            EditorGUI.indentLevel++;
            Localization.SelectLanguageGUI();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if(Localization.GetCurrentCode() == "ja-JP" && !string.IsNullOrEmpty(ChangeLogViewer.instance.changelogJp))
            {
                EditorGUILayout.LabelField(ChangeLogViewer.instance.changelogJp, style);
            }
            else if(!string.IsNullOrEmpty(ChangeLogViewer.instance.changelogEn))
            {
                EditorGUILayout.LabelField(ChangeLogViewer.instance.changelogEn, style);
            }
            else
            {
                EditorGUILayout.LabelField(Localization.S("inspector.changelogLoadFailed"));
            }
            EditorGUILayout.EndScrollView();

            EditorGUI.indentLevel--;
        }
    }
}
