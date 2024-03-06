using System;
using UnityEngine;
using UnityEngine.Animations;

namespace jp.lilxyzw.lilycalinventory.runtime
{
    [AddComponentMenu(ConstantValues.COMPONENTS_BASE + nameof(Comment))]
    [HelpURL(ConstantValues.URL_DOCS_COMPONENT + nameof(Comment))]
    internal class Comment : AvatarTagComponent
    {
        [NotKeyable] [LILLocalize] public MessageType messageType;
        [NotKeyable] public LanguageAndText[] comments = new LanguageAndText[]{};

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
