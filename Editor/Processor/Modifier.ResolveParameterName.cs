using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal static partial class Processor
    {
        private partial class Modifier
        {
            internal static void ResolveParameterNames(AnimatorController controller, ItemToggler[] togglers, CostumeChanger[] costumeChangers, SmoothChanger[] smoothChangers, Preset[] presets)
            {
                // AnimatorControllerとExpressionParametersから既存のパラメーター名を取得
                var duplicates = new Dictionary<string, HashSet<Object>>();
                var parameterNames = new HashSet<string>();
                if(controller.parameters != null) parameterNames.UnionWith(controller.parameters.Select(p => p.name));
                #if LIL_VRCSDK3A
                if(ctx.AvatarDescriptor.expressionParameters) ctx.AvatarDescriptor.expressionParameters.parameters.Select(p => p.name);
                #endif

                // 重複しない名前を決定しつつ既存のパラメーターのリストに追加
                string ResolveName(string name, bool autoFixDuplicate, Object component)
                {
                    var parameterName = name;
                    if(autoFixDuplicate)
                    {
                        int i = 0;
                        while(!parameterNames.Add(parameterName)) parameterName = $"{name} ({++i})";
                    }
                    else
                    {
                        if(!parameterNames.Add(parameterName)) duplicates.GetOrAdd(parameterName).Add(component);
                    }
                    return parameterName;
                }

                foreach(var c in togglers) c.parameterName = ResolveName(c.menuName, c.autoFixDuplicate, c);
                foreach(var c in smoothChangers) c.parameterName = ResolveName(c.menuName, c.autoFixDuplicate, c);
                foreach(var c in costumeChangers)
                {
                    c.parameterName = ResolveName(c.menuName, c.autoFixDuplicate, c);
                    c.parameterNameLocal = ResolveName($"{c.parameterName}_Local", c.autoFixDuplicate, c);
                    c.parameterNameBits = Enumerable.Range(0, c.costumes.Length).Select(i => ResolveName($"{c.parameterName}_Bool{i}", c.autoFixDuplicate, c)).ToArray();
                }
                foreach(var c in presets) c.parameterName = ResolveName(c.menuName, c.autoFixDuplicate, c);

                foreach(var kv in duplicates)
                {
                    if(controller.parameters.Any(p => p.name == kv.Key)) kv.Value.Add(controller);
                    #if LIL_VRCSDK3A
                    if(ctx.AvatarDescriptor.expressionParameters && ctx.AvatarDescriptor.expressionParameters.parameters.Any(p => p.name == kv.Key))
                        kv.Value.Add(ctx.AvatarDescriptor.expressionParameters);
                    #endif
                    ErrorHelper.Report("dialog.error.parameterDuplication", kv.Value);
                }
            }
        }
    }
}
