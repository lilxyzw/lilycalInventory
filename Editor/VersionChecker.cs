using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

namespace jp.lilxyzw.lilycalinventory
{
    using System;
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
                    label = new GUIContent($"{ConstantValues.TOOL_NAME} {instance.current} => {instance.latest}");
                    style = GUIHelper.boldRedStyle;
                }
                else
                {
                    label = new GUIContent($"{ConstantValues.TOOL_NAME} {instance.current}");
                    style = EditorStyles.boldLabel;
                }
            }
            EditorGUILayout.LabelField(label, style);
        }

        [InitializeOnLoadMethod]
        private static void VersionCheck()
        {
            if(instance.current == null)
            {
                EditorApplication.delayCall -= GetCurrentVersion;
                EditorApplication.delayCall += GetCurrentVersion;
                CoroutineHandler.StartStaticCoroutine(GetLatestVersionInfo());
            }
        }

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

        private static IEnumerator GetLatestVersionInfo()
        {
            using(UnityWebRequest webRequest = UnityWebRequest.Get(ConstantValues.URL_PACKAGE_JSON))
            {
                yield return webRequest.SendWebRequest();
                #if UNITY_2020_2_OR_NEWER
                if(webRequest.result != UnityWebRequest.Result.ConnectionError)
                #else
                if(!webRequest.isNetworkError)
                #endif
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
