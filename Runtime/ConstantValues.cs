namespace jp.lilxyzw.lilycalinventory.runtime
{
    // URLやGUIDなどの定数はここに集約して、変更があった場合に更新しやすくします。
    internal class ConstantValues
    {
        internal const string TOOL_NAME = "lilycalInventory";
        internal const string PACKAGE_NAME = "lilycalinventory";
        internal const string PACKAGE_NAME_FULL = "jp.lilxyzw." + PACKAGE_NAME;
        internal const string COMPONENTS_BASE = TOOL_NAME + "/LI ";
        internal const string URL_DOCS_BASE = "https://lilxyzw.github.io/lilycalInventory/redirect#";
        internal const string URL_DOCS_COMPONENT = URL_DOCS_BASE + "docs/components/";
        internal const string URL_PACKAGE_JSON = "https://raw.githubusercontent.com/lilxyzw/lilycalInventory/main/package.json";
        internal const string URL_CHANGELOG_EN = "https://raw.githubusercontent.com/lilxyzw/lilycalInventory/main/CHANGELOG.md";
        internal const string URL_CHANGELOG_JP = "https://raw.githubusercontent.com/lilxyzw/lilycalInventory/main/CHANGELOG_JP.md";

        internal const string GUID_ICON_NEXT = "defba5627489c4648afe871f388469b1";
        internal const string GUID_LOCALIZATION = "d54616bccdc07254a998850242066cc6";
        internal const string GUID_PACKAGE = "38b330de5f5bc1148a6eace5583dbb77";
    }
}
