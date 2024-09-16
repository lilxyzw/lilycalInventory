using UnityEngine;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // メニューのフォルダとなるコンポーネント
    [DisallowMultipleComponent]
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(MenuFolder))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "menufolder")]
    public class MenuFolder : MenuBaseComponent
    {
    }
}
