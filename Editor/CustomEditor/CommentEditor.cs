using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    // コメント表示だけ速度を考慮してUIElementsを使用
    [CustomEditor(typeof(Comment))]
    internal class CommentEditor : Editor
    {
        private static bool isEditing = false;
        private VisualElement veRoot;
        private VisualElement veEditmode;
        private VisualElement veNoComment;
        private VisualElement veComment;
        private VisualElement veCommentRoot;
        private VisualElement veButtonEdit;
        private VisualElement veButtonApply;

        void OnDisable()
        {
            isEditing = false;
        }

        // 初期化
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
            using var messageType = serializedObject.FindProperty("messageType");
            using var comments = serializedObject.FindProperty("comments");

            if(comments.arraySize == 0)
            {
                veNoComment.style.display = DisplayStyle.Flex;
                veComment.style.display = DisplayStyle.None;
                return;
            }

            veNoComment.style.display = DisplayStyle.None;
            veComment.style.display = DisplayStyle.Flex;

            var currentCode = Localization.GetCurrentCode();
            using var end = comments.GetEndProperty();
            var comment = comments.GetArrayElementAtIndex(0);
            using var first = comment.Copy();
            bool isFound = false;
            while(comment.NextVisible(false) && !SerializedProperty.EqualContents(comment, end))
            {
                if(!comment.GetStringInProperty("langcode").Equals(currentCode, StringComparison.OrdinalIgnoreCase)) continue;
                isFound = true;
                break;
            }
            if(!isFound) comment = first;

            veCommentRoot.Clear();
            if(messageType.intValue == (int)Comment.MessageType.Markdown)
            {
                veCommentRoot.Add(MarkdownViewer.Draw(comment.GetStringInProperty("text")));
            }
            else
            {
                veCommentRoot.Add(new CustomHelpBox(comment.GetStringInProperty("text"), (MessageType)messageType.intValue, true));
            }
        }

        // 初期化
        private void UpdateGUI()
        {
            veRoot.Clear();
            veEditmode.Clear();
            veNoComment.Clear();
            veComment.Clear();
            veCommentRoot.Clear();

            // 編集用のGUI
            veEditmode.Add(new CustomHelpBox(Localization.S("inspector.commentInformation"), MessageType.Info));
            veEditmode.Add(new IMGUIContainer(() => {
                using var messageType = serializedObject.FindProperty("messageType");
                using var comments = serializedObject.FindProperty("comments");
                GUIHelper.AutoField(messageType);
                GUIHelper.AutoField(comments);
                serializedObject.ApplyModifiedProperties();
            }));

            // コメントが無い場合のGUI
            veNoComment.Add(new Label(Localization.S("inspector.commentNo")));

            // コメントがある場合のGUI
            veComment.Add(new Label(Localization.S("inspector.commentRoot")));
            veComment.Add(veCommentRoot);

            // 編集ボタンと適用ボタン
            veButtonEdit = new Button(() => {
                isEditing = !isEditing;
                ToggleGUI();
            }){
                text = Localization.S("inspector.edit")
            };

            veButtonApply = new Button(() => {
                isEditing = !isEditing;
                ToggleGUI();
            }){
                text = Localization.S("inspector.apply")
            };

            // Rootに各要素を追加
            veRoot.Add(new IMGUIContainer(() => {if(Localization.SelectLanguageGUI()) UpdateGUI();}));
            veRoot.Add(veEditmode);
            veRoot.Add(veNoComment);
            veRoot.Add(veComment);
            veRoot.Add(veButtonEdit);
            veRoot.Add(veButtonApply);

            ToggleGUI();
        }

        // 編集モードと閲覧モードでGUIの切り替え
        void ToggleGUI()
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
            using var langcode = property.FPR(PROP_LANGCODE);
            using var text = property.FPR(PROP_TEXT);

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
            using var langcode = property.FPR(PROP_LANGCODE);
            using var text = property.FPR(PROP_TEXT);
            return GUIHelper.GetAutoFieldHeight(langcode) + GUIHelper.GetAutoFieldHeight(text);
        }
    }

    // テキストを選択可能なヘルプボックス
    // UIElementsだと複雑化してしまうためIMGUIで代替
    internal class CustomHelpBox : VisualElement
    {
        private static GUIStyle styleText;
        private static bool isInitialized = false;
        internal CustomHelpBox(string label, MessageType type, bool isComment = false)
        {
            // 通常はこっちで普通のヘルプボックスを表示
            if(!isComment)
            {
                Add(new IMGUIContainer(() => EditorGUILayout.HelpBox(label, type)));
                return;
            }

            // コメントである場合はテキストを選択可能なヘルプボックスを表示
            // HTML構文のサポートを行ったり、文字サイズを通常のラベルに合わせて読みやすくしたり
            if(!isInitialized)
            {
                styleText = new GUIStyle(EditorStyles.label){richText = true, wordWrap = true};
                styleText.SetColors(styleText.normal.textColor);
            }

            Add(new IMGUIContainer(() => {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                if(type != MessageType.None)
                {
                    // アイコンが必要な場合は表示
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
