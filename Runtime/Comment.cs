using System;
using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    // コメントを残すコンポーネント
    // これ自体に機能はなく、PrefabやGameObjectに説明を残しておくことを想定しています。
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(Comment))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + "comment")]
    public class Comment : AvatarTagComponent
    {
        [NotKeyable] [LILLocalize] [SerializeField] internal MessageType messageType;
        [NotKeyable] [SerializeField] internal LanguageAndText[] comments = new LanguageAndText[]{};

        [Serializable]
        internal struct LanguageAndText
        {
            [LILLocalize] public string langcode;
            [TextArea(3,100)] [LILLocalize] public string text;
        }

        [Serializable]
        internal enum MessageType
        {
            None,
            Info,
            Warning,
            Error,
            Markdown
        }
    }
}
