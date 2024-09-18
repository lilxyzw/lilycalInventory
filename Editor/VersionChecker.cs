using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal class VersionChecker : ScriptableSingleton<VersionChecker>
    {
        [SerializeField] internal SemVerParser current = null;
        [SerializeField] internal SemVerParser latest = null;
        private static GUIContent label = null;
        private static GUIStyle style = null;

        internal static void DrawGUI()
        {
            if(label == null)
            {
                if(instance.latest > instance.current)
                {
                    // 更新がある場合は最新バージョンも合わせて表示、赤字で強調
                    label = new GUIContent($"{ConstantValues.TOOL_NAME} {instance.current} => {instance.latest}");
                    style = GUIHelper.boldRedStyle;
                }
                else
                {
                    // ツール名+バージョン番号を表示
                    label = new GUIContent($"{ConstantValues.TOOL_NAME} {instance.current}");
                    style = EditorStyles.boldLabel;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, style);
            if(GUILayout.Button(Localization.G("inspector.changelog"))) ChangeLogWindow.Init();
            EditorGUILayout.EndHorizontal();
        }

        [InitializeOnLoadMethod]
        private static void VersionCheck()
        {
            if(instance.current == null)
            {
                IEnumerator Coroutine()
                {
                    yield return GetLatestVersionInfo();
                    yield return ChangeLogViewer.GetChangelogEn();
                    yield return ChangeLogViewer.GetChangelogJp();
                }

                CoroutineHandler.StartStaticCoroutine(Coroutine());
            }
            // package.jsonをここでは読み込まずdelayCallで安全に読み込む
            EditorApplication.delayCall -= GetCurrentVersion;
            EditorApplication.delayCall += GetCurrentVersion;
        }

        // インストールしているバージョンを取得
        private static void GetCurrentVersion()
        {
            EditorApplication.delayCall -= GetCurrentVersion;
            try
            {
                instance.current = new SemVerParser(JsonUtility.FromJson<PackageInfo>(File.ReadAllText(AssetDatabase.GUIDToAssetPath(ConstantValues.GUID_PACKAGE))).version);
            }
            catch(Exception e)
            {
                instance.current = new SemVerParser("0.0.0");
                throw e;
            }
        }

        // 最新版をGitHubから取得
        private static IEnumerator GetLatestVersionInfo()
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(ConstantValues.URL_PACKAGE_JSON);
            yield return webRequest.SendWebRequest();
            if(webRequest.result != UnityWebRequest.Result.ConnectionError)
            {
                try
                {
                    instance.latest = new SemVerParser(JsonUtility.FromJson<PackageInfo>(webRequest.downloadHandler.text).version);
                }
                catch(Exception e)
                {
                    instance.latest = new SemVerParser("0.0.0");
                    throw e;
                }
            }
        }

        private class PackageInfo
        {
            public string version;
        }
    }

    internal class CoroutineHandler : MonoBehaviour
    {
        private static CoroutineHandler m_Instance;
        private static CoroutineHandler Instance => m_Instance ? m_Instance : m_Instance = new GameObject("CoroutineHandler"){hideFlags = HideFlags.HideAndDontSave}.AddComponent<CoroutineHandler>();
        private void OnDisable(){if(m_Instance) Destroy(m_Instance.gameObject);}
        internal static Coroutine StartStaticCoroutine(IEnumerator coroutine) => Instance.StartCoroutine(coroutine);
    }
}
