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
        [NotKeyable] [SerializeField] internal PresetItem[] presetItems = new PresetItem[]{};
    }

    [Serializable]
    internal class PresetItem
    {
        public MenuBaseComponent obj;
        public float value = 0;
    }
}
