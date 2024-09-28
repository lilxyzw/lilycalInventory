using System.Collections.Generic;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    internal class InternalMenu
    {
        internal InternalMenuType type;
        internal string name;
        internal Texture2D icon;
        internal string parameterName;
        internal float value;
        internal List<InternalMenu> menus = new();

        internal static InternalMenu CreateFolder(string name, Texture2D icon)
            => new InternalMenu(){type = InternalMenuType.Folder, name = name, icon = icon};

        internal static InternalMenu CreateToggle(string name, Texture2D icon, string parameterName, float value = 1)
            => new InternalMenu(){type = InternalMenuType.Toggle, name = name, icon = icon, parameterName = parameterName, value = value};

        internal static InternalMenu CreateSlider(string name, Texture2D icon, string parameterName, float value = 1)
            => new InternalMenu(){type = InternalMenuType.Slider, name = name, icon = icon, parameterName = parameterName, value = value};

        internal static InternalMenu CreateTrigger(string name, Texture2D icon, string parameterName, float value = 1)
            =>  new InternalMenu(){type = InternalMenuType.Trigger, name = name, icon = icon, parameterName = parameterName, value = value};
    }

    internal enum InternalMenuType
    {
        Folder,
        Toggle,
        Slider,
        Trigger
    }
}
