using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    [Serializable]
    internal class AsmdefReader
    {
        public VersionDefines[] versionDefines;
        private static AsmdefReader asmdef_LI;
        internal static AsmdefReader Asmdef_LI => asmdef_LI == null ? asmdef_LI = FromGUID("1cd7a51e46ac2b24d97d50a5e7b12d7a") : asmdef_LI;

        internal static AsmdefReader FromGUID(string guid)
        {
            return JsonUtility.FromJson<AsmdefReader>(File.ReadAllText(AssetDatabase.GUIDToAssetPath(guid)));
        }

        [Serializable]
        internal class VersionDefines
        {
            public string name;
            public string expression;
            public string define;
        }
    }
}
