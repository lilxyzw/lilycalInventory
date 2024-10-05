using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static class ParameterViewer
    {
        private static bool isInitialized = false;
        private static bool isExpandedDetails = false;
        private static GameObject avatarRoot;
        private static (string name, Color color, int cost, Object[] objects)[] plugins = new (string name, Color color, int cost, Object[] objects)[2];
        private static int costSum;
        private static Dictionary<string, bool> isExpandeds = new();

        #if LIL_VRCSDK3A
        private static int costMax = VRChatHelper.costMax;
        private static int costBool = VRChatHelper.costBool;
        private static int costInt = VRChatHelper.costInt;
        private static int costFloat = VRChatHelper.costFloat;
        #else
        private const int costMax = 256;
        private const int costBool = 1;
        private const int costInt = 8;
        private const int costFloat = 8;
        #endif

        internal static void Draw(MenuBaseComponent component)
        {
            Update(component);
            if(!avatarRoot) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            // トータルのコスト
            if(isExpandedDetails = EditorGUILayout.Foldout(isExpandedDetails, $"{Localization.S("inspector.ParameterViewer.memoryUsed")}: {costSum} / {costMax} ({Localization.S("inspector.ParameterViewer.memoryRemaining")}: {costMax - costSum})"))
            {
                foreach(var (name, color, cost, objects) in plugins)
                {
                    if(!isExpandeds.ContainsKey(name)) isExpandeds[name] = false;

                    EditorGUI.indentLevel++;
                    // 各Pluginのコスト
                    if(isExpandeds[name] = EditorGUILayout.Foldout(isExpandeds[name], $"{name}: {cost}"))
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        foreach(var c in objects) EditorGUILayout.ObjectField(c, typeof(Object), true);
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;

            // ここからはグラフの表示
            var position = EditorGUILayout.GetControlRect(GUILayout.Height(8));
            var rect = position;
            EditorGUI.DrawRect(rect, new Color(0.5f,0.5f,0.5f,0.5f));

            foreach(var (name, color, cost, objects) in plugins)
            {
                rect.width = position.width * ((float)cost / costMax);
                EditorGUI.DrawRect(rect, color);
                rect.x = rect.xMax;
            }
            EditorGUILayout.EndVertical();
        }

        internal static void Reset()
        {
            isInitialized = false;
            avatarRoot = null;
        }

        private static void Update(MenuBaseComponent component)
        {
            if(isInitialized) return;
            isInitialized = true;

            // アバターでない場合は何も表示しない
            var root = component.gameObject.GetAvatarRoot();
            if(!root) return;
            avatarRoot = root.gameObject;

            #if LIL_NDMF
            // NDMFがある場合はNDMFから取得
            plugins = ParameterInfo.ForUI.GetParametersForObject(avatarRoot).GroupBy(p => p.Plugin)
            .Select(group => (
                group.Key.DisplayName,
                group.Key.ThemeColor??Color.red,
                group.Sum(p => p.BitUsage),
                group.Select(p => p.Source as Object).Distinct().ToArray()
            )).ToArray();
            #else
            #if LIL_VRCSDK3A
            // アバターのコスト
            plugins[0].name = Localization.S("inspector.ParameterViewer.memoryAvatar");
            plugins[0].color = new Color(0.203f, 0.764f, 0.450f);
            plugins[0].cost = 0;
            plugins[0].objects = null;
            var descriptor = avatarRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if(descriptor && descriptor.expressionParameters)
            {
                plugins[0].cost = descriptor.expressionParameters.CalcTotalCost();
                plugins[0].objects = new[]{descriptor.expressionParameters};
            }
            #endif

            // lilycalInventoryのコスト
            plugins[1].name = "lilycalInventory";
            plugins[1].color = new Color(0.572f, 0.549f, 0.858f);
            var components = avatarRoot.GetActiveComponentsInChildren<MenuBaseComponent>(true).Where(c => !(c is MenuFolder) && !(c is AutoDresserSettings) && c.IsEnabledInBuild());
            var autoDressers = components.Where(c => c is AutoDresser);
            var props = components.Where(c => c is Prop);
            var itemTogglers = components.Where(c => c is ItemToggler);
            var costumeChangers = components.Where(c => c is CostumeChanger);
            var smoothChangers = components.Where(c => c is SmoothChanger);

            plugins[1].cost = costBool * (props.Count() + itemTogglers.Count())
                + costumeChangers.Select(c => c as CostumeChanger).Sum(c => ObjHelper.ToNBitInt(c.costumes.Length))
                + costFloat * smoothChangers.Count()
                + (autoDressers.Count() > 0 ? ObjHelper.ToNBitInt(autoDressers.Count()) : 0);
            plugins[1].objects = autoDressers.Union(props).Union(itemTogglers).Union(costumeChangers).Union(smoothChangers).ToArray();
            #endif

            costSum = plugins.Sum(p => p.cost);
        }
    }
}
