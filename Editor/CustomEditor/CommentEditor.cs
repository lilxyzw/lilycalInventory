using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;
    using UnityEditor.UIElements;

    [CustomEditor(typeof(Comment))]
    internal class CommentEditor : Editor
    {
        private static bool isEditing = false;
        private static Texture2D m_IconInfo;
        private static Texture2D m_IconWarn;
        private static Texture2D m_IconError;
        private static Texture2D iconInfo => m_IconInfo ? m_IconInfo : m_IconInfo = (Texture2D)EditorGUIUtility.LoadRequired("console.infoicon");
        private static Texture2D iconWarn => m_IconWarn ? m_IconWarn : m_IconWarn = (Texture2D)EditorGUIUtility.LoadRequired("console.warnicon");
        private static Texture2D iconError => m_IconError ? m_IconError : m_IconError = (Texture2D)EditorGUIUtility.LoadRequired("console.erroricon");
        private VisualElement veRoot;
        private VisualElement veEditmode;
        private VisualElement veNoComment;
        private VisualElement veComment;
        private VisualElement veCommentRoot;
        private VisualElement veButtonEdit;
        private VisualElement veButtonApply;

        void OnDisable()
        {
            GUIHelper.ResetList();
            isEditing = false;
        }

        public override VisualElement CreateInspectorGUI()
        {
            veRoot = new VisualElement();
            veEditmode = new VisualElement();
            veNoComment = new VisualElement();
            veComment = new VisualElement();
            veCommentRoot = new VisualElement();
            veRoot.Bind(serializedObject);
            UpdateGUI();
            return veRoot;
        }

        private void DrawComment()
        {
            var messageType = serializedObject.FindProperty("messageType");
            var comments = serializedObject.FindProperty("comments");

            if(comments.arraySize == 0)
            {
                veNoComment.style.display = DisplayStyle.Flex;
                veComment.style.display = DisplayStyle.None;
                return;
            }

            veNoComment.style.display = DisplayStyle.None;
            veComment.style.display = DisplayStyle.Flex;

            var currentCode = Localization.GetCurrentCode();
            var end = comments.GetEndProperty();
            var comment = comments.GetArrayElementAtIndex(0);
            var first = comment.Copy();
            bool isFound = false;
            while(comment.NextVisible(false) && !SerializedProperty.EqualContents(comment, end))
            {
                if(!comment.FPR("langcode").stringValue.Equals(currentCode, StringComparison.OrdinalIgnoreCase)) continue;
                isFound = true;
                break;
            }
            if(!isFound) comment = first;

            veCommentRoot.Clear();
            if(messageType.intValue == (int)Comment.MessageType.Markdown)
            {
                veCommentRoot.Add(MarkdownViewer.Draw(comment.FPR("text").stringValue));
            }
            else
            {
                veCommentRoot.Add(new CustomHelpBox(comment.FPR("text").stringValue, (MessageType)messageType.intValue, true));
            }
        }

        private void UpdateGUI()
        {
            veRoot.Clear();
            veEditmode.Clear();
            veNoComment.Clear();
            veComment.Clear();
            veCommentRoot.Clear();

            veEditmode.Add(new CustomHelpBox(Localization.S("inspector.commentInformation"), MessageType.Info));
            veEditmode.Add(new IMGUIContainer(() => {
                GUIHelper.AutoField(serializedObject.FindProperty("messageType"));
                GUIHelper.AutoField(serializedObject.FindProperty("comments"));
                serializedObject.ApplyModifiedProperties();
            }));
            veNoComment.Add(new Label(Localization.S("inspector.commentNo")));
            veComment.Add(new Label(Localization.S("inspector.commentRoot")));
            veComment.Add(veCommentRoot);

            veButtonEdit = new Button(() => {
                isEditing = !isEditing;
                UpdateUI();
            }){
                text = Localization.S("inspector.edit")
            };

            veButtonApply = new Button(() => {
                isEditing = !isEditing;
                UpdateUI();
            }){
                text = Localization.S("inspector.apply")
            };

            veRoot.Add(new IMGUIContainer(() => {if(Localization.SelectLanguageGUI()) UpdateGUI();}));
            veRoot.Add(veEditmode);
            veRoot.Add(veNoComment);
            veRoot.Add(veComment);
            veRoot.Add(veButtonEdit);
            veRoot.Add(veButtonApply);

            UpdateUI();
        }

        void UpdateUI()
        {
            if(isEditing)
            {
                veEditmode.style.display = DisplayStyle.Flex;
                veNoComment.style.display = DisplayStyle.None;
                veComment.style.display = DisplayStyle.None;
                veButtonEdit.style.display = DisplayStyle.None;
                veButtonApply.style.display = DisplayStyle.Flex;
            }
            else
            {
                veEditmode.style.display = DisplayStyle.None;
                veButtonEdit.style.display = DisplayStyle.Flex;
                veButtonApply.style.display = DisplayStyle.None;
                DrawComment();
            }
        }
    }

    [CustomPropertyDrawer(typeof(Comment.LanguageAndText), true)]
    internal class LanguageAndTextDrawer : PropertyDrawer
    {
        private const string PROP_LANGCODE = nameof(Comment.LanguageAndText.langcode);
        private const string PROP_TEXT = nameof(Comment.LanguageAndText.text);
        private static string[] codes = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(c => c.Name).ToArray();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var langcode = property.FPR(PROP_LANGCODE);
            var text = property.FPR(PROP_TEXT);

            var code = langcode.stringValue;
            if(codes.Any(c => c.Equals(code, StringComparison.OrdinalIgnoreCase)))
            {
                EditorGUI.PropertyField(position.SingleLine(), langcode);
            }
            else
            {
                var colors = EditorStyles.textField.GetColors();
                EditorStyles.textField.SetColors(Color.red);
                EditorGUI.PropertyField(position.SingleLine(), langcode);
                EditorStyles.textField.SetColors(colors);
            }
            position.NewLine();
            EditorGUI.PropertyField(position.SetHeight(text), text, Localization.G(text));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GUIHelper.GetAutoFieldHeight(property.FPR(PROP_LANGCODE)) + GUIHelper.GetAutoFieldHeight(property.FPR(PROP_TEXT));
        }
    }

    internal class CustomHelpBox : VisualElement
    {
        private static GUIStyle styleText;
        private static bool isInitialized = false;
        internal CustomHelpBox(string label, MessageType type, bool isComment = false)
        {
            if(!isComment)
            {
                Add(new IMGUIContainer(() => EditorGUILayout.HelpBox(label, type)));
                return;
            }
            if(!isInitialized)
            {
                styleText = new GUIStyle(EditorStyles.label){richText = true, wordWrap = true};
                styleText.SetColors(styleText.normal.textColor);
            }
            Add(new IMGUIContainer(() => {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                if(type != MessageType.None)
                {
                    var icon = GetIconContent(type);
                    EditorGUILayout.BeginVertical(GUILayout.Width(icon.image.width));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(icon, GUIStyle.none, GUILayout.Width(icon.image.width), GUILayout.Height(icon.image.height));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.SelectableLabel(GUILayoutUtility.GetRect(new GUIContent(label), styleText), label, styleText);
                EditorGUILayout.EndHorizontal();
            }));
        }

        private GUIContent GetIconContent(MessageType messageType)
        {
            switch(messageType)
            {
                case MessageType.Info: return EditorGUIUtility.IconContent("console.infoicon");
                case MessageType.Warning: return EditorGUIUtility.IconContent("console.warnicon");
                case MessageType.Error: return EditorGUIUtility.IconContent("console.erroricon");
                default: return null;
            };
        }
    }
}
