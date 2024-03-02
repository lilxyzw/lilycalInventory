using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal static partial class ObjHelper
    {
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
                var descriptor = gameObject.GetComponentInParentInRoot<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(null);
                if(descriptor) return descriptor.transform;
            #else
                var animator = gameObject.GetComponentInParentInRoot<Animator>(null);
                if(animator) return animator.transform;
            #endif
            return null;
        }

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

        internal static T[] SelectComponents<T>(this Component[] components) where T : MonoBehaviour
        {
            return components.Select(c => c as T).Where(c => c).ToArray();
        }

        internal static T LoadAssetByGUID<T>(string guid) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

        internal static T[] GetActiveComponentsInChildren<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            return gameObject.GetComponentsInChildren<T>(true).Where(c => c.enabled && !c.IsEditorOnly()).ToArray();
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
    }
}
