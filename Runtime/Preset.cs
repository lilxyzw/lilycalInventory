using System;
using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // 付けたオブジェクトをオンオフするコンポーネント
    // ビルド時にItemTogglerに変換されます。
    [DisallowMultipleComponent]
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(Preset))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "preset")]
    public class Preset : MenuBaseComponent
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal bool autoFixDuplicate = true;
        [NotKeyable] [SerializeField] internal PresetItem[] presetItems = new PresetItem[]{};

        [System.NonSerialized] internal string parameterName;
    }

    [Serializable]
    internal class PresetItem
    {
        public MenuBaseComponent obj;
        public float value = 0;
    }
}
