using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal class AvatarScanner
    {
        private static bool isInitialized = false;
        private static GameObject avatarRoot;
        private static Dictionary<Object, IEnumerable<AnimationClip>> animatedObjects = new();
        private static GUIContent HelpContentEditorOnly => new(Localization.S("inspector.targetEditorOnly"), EditorGUIUtility.IconContent("console.warnicon").image);
        private static GUIContent HelpContentAnimationClip => new(Localization.S("inspector.targetAnimationClip"), EditorGUIUtility.IconContent("console.warnicon").image);

        internal static void Draw(Object[] targets)
        {
            // EditorOnlyになっている場合にビルド時に無視される旨の警告を表示
            if(targets.All(t => ((Component)t).IsEditorOnly()))
            {
                EditorGUILayout.HelpBox(HelpContentEditorOnly.text, MessageType.Warning);
            }
            // AnimationClipで操作されている場合に警告を表示
            var clips = targets.Select(t => Get(t)).Where(a => a != null).SelectMany(a => a).Distinct();
            if(clips.Any())
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.HelpBox(HelpContentAnimationClip.text, MessageType.Info);
                GUI.enabled = false;
                foreach(var clip in clips) EditorGUILayout.ObjectField(clip, typeof(AnimationClip), false);
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }
        }

        internal static void Draw(Rect position, Object obj)
        {
            // EditorOnlyになっている場合にビルド時に無視される旨の警告を表示
            if((obj is GameObject g && g.IsEditorOnly()) || (obj is Component c && c.IsEditorOnly()))
            {
                var yMax = position.yMax;
                position.height = EditorStyles.helpBox.CalcHeight(HelpContentEditorOnly, position.width);
                EditorGUI.HelpBox(position, HelpContentEditorOnly.text, MessageType.Warning);
                position.NewLine();
                position.yMax = yMax;
            }
            // AnimationClipで操作されている場合に警告を表示
            var clips = Get(obj);
            if(clips != null)
            {
                if(Event.current.type == EventType.Repaint) EditorStyles.helpBox.Draw(position, false, false, false, false);
                position.xMin += EditorStyles.helpBox.padding.left;
                position.xMax -= EditorStyles.helpBox.padding.right;
                position.yMin += EditorStyles.helpBox.padding.top;

                position.height = EditorStyles.helpBox.CalcHeight(HelpContentAnimationClip, position.width);
                EditorGUI.HelpBox(position, HelpContentAnimationClip.text, MessageType.Info);
                position.NewLine();
                position.height = GUIHelper.propertyHeight;
                GUI.enabled = false;
                foreach(var clip in clips)
                {
                    EditorGUI.ObjectField(position, clip, typeof(AnimationClip), false);
                    position.NewLine();
                }
                GUI.enabled = true;
            }
        }

        internal static float Height(Object obj)
        {
            float width = EditorGUIUtility.currentViewWidth - 90 - EditorStyles.helpBox.padding.left - EditorStyles.helpBox.padding.right;
            float height = 0;
            // EditorOnlyになっている場合にビルド時に無視される旨の警告を表示
            if((obj is GameObject g && g.IsEditorOnly()) || (obj is Component c && c.IsEditorOnly()))
            {
                height += EditorStyles.helpBox.CalcHeight(HelpContentEditorOnly, width);
            }
            // AnimationClipで操作されている場合に警告を表示
            var clips = Get(obj);
            if(clips != null)
            {
                height += EditorStyles.helpBox.padding.top + EditorStyles.helpBox.padding.bottom;
                width -= EditorStyles.helpBox.padding.left - EditorStyles.helpBox.padding.right;
                height += EditorStyles.helpBox.CalcHeight(HelpContentAnimationClip, width) + GUIHelper.GetSpaceHeight(2);

                var animCount = clips.Count();
                height += animCount * GUIHelper.propertyHeight + GUIHelper.GetSpaceHeight(animCount);
            }
            return height;
        }

        internal static void Update(Component component)
        {
            if(!isInitialized)
            {
                isInitialized = true;
                avatarRoot = null;
                animatedObjects.Clear();
                var root = component.gameObject.GetAvatarRoot();
                if(!root) return;
                avatarRoot = root.gameObject;

                var controllers = new HashSet<RuntimeAnimatorController>();

                var animator = avatarRoot.GetComponent<Animator>();
                if(animator && animator.runtimeAnimatorController) controllers.Add(animator.runtimeAnimatorController);

                #if LIL_VRCSDK3A
                VRChatHelper.GetAnimatorControllers(avatarRoot, controllers);
                #endif

                animatedObjects = controllers.Where(c => c).SelectMany(c => c.animationClips).Distinct()
                    .SelectMany(c => AnimationUtility.GetCurveBindings(c).Select(b => (c,b)))
                    .Select(kv => (kv.c, AnimationUtility.GetAnimatedObject(avatarRoot, kv.b)))
                    .Where(kv => kv.Item2).GroupBy(kv => kv.Item2)
                    .ToDictionary(g => g.Key, g => g.Select(kv => kv.c));
            }
        }

        internal static void Reset()
        {
            isInitialized = false;
        }

        private static IEnumerable<AnimationClip> Get(Object obj)
        {
            if(!obj || !isInitialized || !avatarRoot || animatedObjects.Count == 0) return null;
            if(animatedObjects.ContainsKey(obj)) return animatedObjects[obj];
            if(obj is GameObject go) return GetParent(go);
            if(obj is Component c)
            {
                if(animatedObjects.ContainsKey(c.gameObject)) return animatedObjects[c.gameObject];
                return GetParent(c.gameObject);
            }
            return null;
        }

        private static IEnumerable<AnimationClip> GetParent(GameObject obj)
        {
            var parent = obj.transform.parent;
            if(!parent) return null;
            if(animatedObjects.ContainsKey(parent.gameObject)) return animatedObjects[parent.gameObject];
            return GetParent(parent.gameObject);
        }
    }
}
