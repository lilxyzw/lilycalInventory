using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;

#if LIL_NDMF
using nadena.dev.ndmf;
#endif

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    internal partial class Modifier
    {
        internal static void ResolveParameterNames(BuildContext ctx, AnimatorController controller, ItemToggler[] togglers, CostumeChanger[] costumeChangers, SmoothChanger[] smoothChangers, Preset[] presets)
        {
            // AnimatorControllerとExpressionParametersから既存のパラメーター名を取得
            var parameterNames = new HashSet<string>();
            if(controller.parameters != null) parameterNames.UnionWith(controller.parameters.Select(p => p.name));
            #if LIL_VRCSDK3A
            if(ctx.AvatarDescriptor.expressionParameters) ctx.AvatarDescriptor.expressionParameters.parameters.Select(p => p.name);
            #endif

            // 重複しない名前を決定しつつ既存のパラメーターのリストに追加
            string ResolveName(string name)
            {
                var parameterName = name;
                int i = 0;
                while(parameterNames.Contains(parameterName))
                    parameterName = $"{name} ({++i})";
                parameterNames.Add(parameterName);
                return parameterName;
            }

            foreach(var c in togglers) c.parameterName = ResolveName(c.menuName);
            foreach(var c in smoothChangers) c.parameterName = ResolveName(c.menuName);
            foreach(var c in costumeChangers)
            {
                c.parameterName = ResolveName(c.menuName);
                c.parameterNameLocal = ResolveName($"{c.parameterName}_Local");
                c.parameterNameBits = Enumerable.Range(0, c.costumes.Length).Select(i => ResolveName($"{c.parameterName}_Bool{i}")).ToArray();
            }
            foreach(var c in presets) c.parameterName = ResolveName(c.menuName);
        }
    }
}
