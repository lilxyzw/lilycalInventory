using System.Linq;
using System.Text;
using UnityEngine;

namespace jp.lilxyzw.avatarmodifier
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

        internal static T[] GetActiveComponents<T>(this Component[] components) where T : MonoBehaviour
        {
            return components.Select(c => c as T).Where(c => c && c.enabled).ToArray();
        }
    }
}
