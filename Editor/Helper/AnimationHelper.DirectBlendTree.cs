using System.Linq;
using jp.lilxyzw.lilycalinventory.runtime;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    // DirectBlendTreeで処理
    // AnimationHelper.Layer.cs と対になっています
    internal static partial class AnimationHelper
    {
        // lilycalInventoryの全DirectBlendTreeの追加先となるレイヤーを作成
        internal static void CreateLayer(AnimatorController controller, out BlendTree root)
        {
            // 常に1に設定されるWeightプロパティを生成
            var parameterName = "Weight";
            if(controller.parameters.Any(p => p.name == parameterName))
            {
                for(int i = 0; i < 100; i++)
                {
                    var nameTemp = $"{parameterName}_{i}";
                    if(controller.parameters.Any(p => p.name == nameTemp)) continue;
                    parameterName = nameTemp;
                    break;
                }
            }
            controller.AddParameter(new AnimatorControllerParameter() { name = parameterName, type = AnimatorControllerParameterType.Float, defaultFloat = 1 });

            // 各コンポーネントで生成されるBlendTreeの追加先のBlendTree
            root = new BlendTree
            {
                blendType = BlendTreeType.Direct,
                blendParameter = parameterName,
                name = "Root",
                useAutomaticThresholds = false
            };

            // BlendTreeの追加先のState
            var state = new AnimatorState
            {
                motion = root,
                name = "Root",
                writeDefaultValues = true
            };

            // Stateの追加先のStateMachine
            var stateMachine = new AnimatorStateMachine();
            stateMachine.AddState(state, stateMachine.entryPosition + new Vector3(200,0,0));
            stateMachine.defaultState = state;

            // StateMachineの追加先のLayer
            var layer = new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = ConstantValues.TOOL_NAME,
                stateMachine = stateMachine
            };
            controller.AddLayer(layer);
        }

        // 子のBlendTreeにパラメーターを設定
        internal static void SetParameter(BlendTree root)
        {
            var children = root.children;
            for(int i = 0; i < children.Length; i++)
                children[i].directBlendParameter = root.blendParameter;
            root.children = children;
        }

        internal static void AddItemTogglerTree(AnimatorController controller, AnimationClip clipDefault, AnimationClip clipChanged, string name, string parameterName, bool defaultValue, BlendTree root)
        {
            var layer = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                blendParameter = parameterName,
                name = name,
                useAutomaticThresholds = true
            };

            // オンオフアニメーションを追加

            // ItemTogglerにはアバターアップロード/リセット時の状態からメニューを操作したときにどういう状態にするかが設定されているため、
            // パラメーターのデフォルト値がfalseならパラメーターがtrueのとき、
            // パラメーターのデフォルト値がtrueならパラメーターがfalseのとき、
            // ItemTogglerに設定されている状態にする必要がある
            // ゆえに、
            // defaultValueがfalseの場合、ItemTogglerに設定されている状態にするのはパラメーターがtrueのときとなり、
            // defaultValueがtrueの場合、ItemTogglerに設定されている状態にするのはパラメーターがfalseのときとなる
            layer.AddChild(!defaultValue ? clipDefault : clipChanged);
            layer.AddChild(!defaultValue ? clipChanged : clipDefault);

            root.AddChild(layer);

            if(!controller.parameters.Any(p => p.name == parameterName))
                controller.AddParameter(new AnimatorControllerParameter() { name = parameterName, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue ? 1 : 0 });
        }

        internal static void AddCostumeChangerTree(AnimatorController controller, AnimationClip[] clips, string name, string parameterName, int defaultValue, BlendTree root)
        {
            var layer = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                blendParameter = parameterName,
                name = name,
                useAutomaticThresholds = false
            };

            // 衣装の数だけアニメーションを追加
            for(int i = 0; i < clips.Length; i++)
                layer.AddChild(clips[i], i);

            root.AddChild(layer);

            if(!controller.parameters.Any(p => p.name == parameterName))
                controller.AddParameter(new AnimatorControllerParameter() { name = parameterName, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });
        }

        internal static void AddSmoothChangerTree(AnimatorController controller, AnimationClip[] clips, float[] frames, string name, string parameterName, float defaultValue, BlendTree root)
        {
            var layer = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                blendParameter = parameterName,
                name = name,
                useAutomaticThresholds = false
            };

            // フレームの数だけアニメーションを追加
            for(int i = 0; i < clips.Length; i++)
                layer.AddChild(clips[i], frames[i]);

            root.AddChild(layer);

            if(!controller.parameters.Any(p => p.name == parameterName))
                controller.AddParameter(new AnimatorControllerParameter() { name = parameterName, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });
        }

        // 複数コンポーネントから操作されるオブジェクト用
        internal static void AddMultiConditionTree(AnimatorController controller, AnimationClip clipDefault, AnimationClip clipChanged, (string name, bool toActive, bool defaultValue)[] bools, (string name, bool[] toActives, int defaultValue)[] ints, BlendTree root, bool isActive)
        {
            var clipActive = isActive ? clipDefault : clipChanged;
            var clipInactive = isActive ? clipChanged : clipDefault;

            AddTree(root);

            void AddTree(BlendTree parent, int depth = 0, int value = 0)
            {
                var index = depth;

                if(index < bools.Length)
                {
                    AddBoolTree(parent, depth, value, bools[index].name, bools[index].toActive, bools[index].defaultValue);
                    return;
                }
                index -= bools.Length;

                if(index < ints.Length)
                {
                    AddIntTree(parent, depth, value, ints[index].name, ints[index].toActives, ints[index].defaultValue);
                    return;
                }
                index -= ints.Length;

                parent.AddChild(clipActive, value);
            }

            // 非アクティブにする条件をor、アクティブにする条件をandにする
            // https://github.com/lilxyzw/lilycalInventory/pull/70#issuecomment-2107029075
            void AddBoolTree(BlendTree parent, int depth, int value, string name, bool toActive, bool defaultValue)
            {
                var layer = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = name,
                    name = name,
                    useAutomaticThresholds = true
                };
                parent.AddChild(layer, value);

                if(!controller.parameters.Any(p => p.name == name))
                    controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue ? 1 : 0 });

                // ItemTogglerにはアバターアップロード/リセット時の状態からメニューを操作したときにどういう状態にするかが設定されているため、
                // パラメーターのデフォルト値がfalseならパラメーターがtrueのとき、
                // パラメーターのデフォルト値がtrueならパラメーターがfalseのとき、
                // ItemTogglerに設定されている状態にする必要がある
                // よって、
                // ItemTogglerに非アクティブが設定されていてパラメーターのデフォルト値がfalseであるか、
                // ItemTogglerにアクティブが設定されていてパラメーターのデフォルト値がtrueである場合、
                // 非アクティブにするのはパラメーターがtrueのときであり、
                // ItemTogglerに非アクティブが設定されていてパラメーターのデフォルト値がtrueであるか、
                // ItemTogglerにアクティブが設定されていてパラメーターのデフォルト値がfalseである場合、
                // 非アクティブにするのはパラメーターがfalseのときである
                // ゆえに、
                // toActiveとdefaultValueが等しい場合、非アクティブにするのはパラメーターがtrueのときとなり、
                // toActiveとdefaultValueが異なる場合、非アクティブにするのはパラメーターがfalseのときとなる
                if(toActive == defaultValue)
                {
                    AddTree(layer, depth + 1);
                    layer.AddChild(clipInactive);
                }
                else
                {
                    layer.AddChild(clipInactive);
                    AddTree(layer, depth + 1);
                }
            }

            // 非アクティブにする条件をor、アクティブにする条件をandにする
            // https://github.com/lilxyzw/lilycalInventory/pull/70#issuecomment-2107029075
            void AddIntTree(BlendTree parent, int depth, int value, string name, bool[] toActives, int defaultValue)
            {
                var layer = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D,
                    blendParameter = name,
                    name = name,
                    useAutomaticThresholds = false
                };
                parent.AddChild(layer, value);

                if(!controller.parameters.Any(p => p.name == name))
                    controller.AddParameter(new AnimatorControllerParameter() { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });

                for(var i = 0; i < toActives.Length; i++)
                {
                    if(!toActives[i]) layer.AddChild(clipInactive, i);
                    else AddTree(layer, depth + 1, i);
                }
            }
        }
    }
}
