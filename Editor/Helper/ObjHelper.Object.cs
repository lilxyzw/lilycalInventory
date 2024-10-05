using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class ObjHelper
    {
        internal static readonly Dictionary<GameObject, string> pathInAvatars = new();

        internal static string TryGetName(this Object obj)
        {
            if(obj) return obj.name;
            return "None";
        }

        internal static T GetComponentInParentInRoot<T>(this GameObject gameObject, Transform root) where T : Object
        {
            var parent = gameObject.transform.parent;
            if(!parent) return null;
            var component = parent.GetComponent<T>();
            if(component) return component;
            if(parent == root) return null;
            return parent.gameObject.GetComponentInParentInRoot<T>(root);
        }

        internal static T GetComponentInParentInAvatar<T>(this GameObject gameObject) where T : Object
        {
            return gameObject.GetComponentInParentInRoot<T>(gameObject.GetAvatarRoot());
        }

        internal static Transform GetAvatarRoot(this GameObject gameObject)
        {
            #if LIL_VRCSDK3A
                if(gameObject.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()) return gameObject.transform;
                var descriptor = gameObject.GetComponentInParentInRoot<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(null);
                if(descriptor) return descriptor.transform;
            #else
                if(gameObject.GetComponent<Animator>()) return gameObject.transform;
                var animator = gameObject.GetComponentInParentInRoot<Animator>(null);
                if(animator) return animator.transform;
            #endif
            return null;
        }

        // AnimationClip用のパス
        private static string GetPathFrom(this GameObject gameObject, Transform root)
        {
            var path = new StringBuilder();
            path.Append(gameObject.name);
            var parent = gameObject.transform.parent;
            while(parent && parent != root)
            {
                path.Insert(0, $"{parent.name}/");
                parent = parent.parent;
            }
            return path.ToString();
        }

        internal static string GetPathInAvatar(this GameObject gameObject)
        {
            return gameObject.GetPathFrom(gameObject.GetAvatarRoot());
        }

        internal static string GetPathInAvatar(this Component component)
        {
            return component.gameObject.GetPathInAvatar();
        }

        internal static string GetPathInAvatarFast(this GameObject gameObject)
        {
            if(pathInAvatars.ContainsKey(gameObject)) return pathInAvatars[gameObject];
            return pathInAvatars[gameObject] = gameObject.GetPathInAvatar();
        }

        internal static string GetPathInAvatarFast(this Component component)
        {
            return component.gameObject.GetPathInAvatarFast();
        }

        // 指定の型に一致するコンポーネントをキャストしつつ取得
        internal static T[] SelectComponents<T>(this Component[] components) where T : MonoBehaviour
        {
            return components.Select(c => c as T).Where(c => c).ToArray();
        }

        // Unityにないので
        internal static T LoadAssetByGUID<T>(string guid) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        // チェックボックスがオンなコンポーネントを取得
        // ついでにEditorOnlyも除外
        internal static T[] GetActiveComponentsInChildren<T>(this GameObject gameObject, bool includeInactive) where T : MonoBehaviour
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive).Where(c => c.IsEnabledInBuild()).ToArray();
        }

        // EditorOnly
        internal static bool IsEditorOnly(Transform obj)
        {
            if(obj.tag == "EditorOnly") return true;
            if(obj.transform.parent == null) return false;
            return IsEditorOnly(obj.transform.parent);
        }

        internal static bool IsEditorOnly(this GameObject obj)
        {
            return IsEditorOnly(obj.transform);
        }

        internal static bool IsEditorOnly(this Component com)
        {
            return IsEditorOnly(com.transform);
        }

        // 有効なコンポーネントか
        internal static bool IsEnabledInBuild(this MonoBehaviour component)
        {
            return component.enabled && !component.IsEditorOnly();
        }

        internal static float GetBlendShapeWeight(this SkinnedMeshRenderer obj, string name)
        {
            if(!obj || !obj.sharedMesh) return 0;
            var index = obj.sharedMesh.GetBlendShapeIndex(name);
            if(index == -1) return 0;
            return obj.GetBlendShapeWeight(index);
        }

        internal static HashSet<TValue> GetOrAdd<TKey,TValue>(this Dictionary<TKey,HashSet<TValue>> dic, TKey key)
        {
            if(!dic.ContainsKey(key)) dic[key] = new HashSet<TValue>();
            return dic[key];
        }
    }
}
