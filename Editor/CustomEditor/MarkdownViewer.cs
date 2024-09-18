using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;
using UnityEngine.TextCore.Text;

namespace jp.lilxyzw.lilycalinventory
{
    internal static class MarkdownViewer
    {
        private static readonly Dictionary<string,List<(MDType,string,int,int)>> mdlist = new();
        private static readonly Encoding encSjis = Encoding.GetEncoding("Shift_JIS");

        internal enum MDType
        {
            p,
            h1,
            h2,
            h3,
            h4,
            h5,
            h6,
            ul,
            ol,
            h1line,
            h2line,
            br
        }

        internal class MDLabel : Label
        {
            private const float FONT_SIZE = 14f;
            private static Dictionary<MouseCursor, Cursor> cursors = new();

            // ----------------------------------------------------------------
            // UIElementsで日本語フォントが壊れているため生成して上書き
            // 以下を参考にして書き直したほうがいいかも
            // https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/LocalizationEditor/LocalizedEditorFontManager.cs
            private static readonly string[] fontNamesEn = {"Inter", "Arial"};
            private static readonly string[] fontNamesJp = {"Yu Gothic UI", "Meiryo UI"};
            private static bool isInitialized = false;
            private static FontAsset m_FontAsset = null;
            private static FontAsset fontAsset => m_FontAsset ? m_FontAsset : m_FontAsset = InitializeFontAsset();
            private static FontDefinition fontDefinition = FontDefinition.FromSDFFont(fontAsset);

            private static FontAsset InitializeFontAsset()
            {
                if(isInitialized) return m_FontAsset;
                isInitialized = true;
                var allFonts = Font.GetOSInstalledFontNames();

                foreach(var fontName in fontNamesEn)
                    if(allFonts.Contains(fontName)) 
                        AddFont(FontAsset.CreateFontAsset(fontName, ""));

                foreach(var fontName in fontNamesJp)
                    if(allFonts.Contains(fontName)) 
                        AddFont(FontAsset.CreateFontAsset(fontName, ""));

                return m_FontAsset;
            }

            private static void AddFont(FontAsset fontAsset)
            {
                if(m_FontAsset)
                {
                    m_FontAsset.fallbackFontAssetTable.Add(fontAsset);
                    return;
                }

                m_FontAsset = fontAsset;
                m_FontAsset.fallbackFontAssetTable = new List<FontAsset>();
            }

            private static void SetFont(IStyle style)
            {
                if(fontAsset) style.unityFontDefinition = fontDefinition;
            }

            // マウスを重ねたときにテキスト選択用のカーソルに変えたいので必要
            private static Cursor GetDefaultCursor(MouseCursor mouseCursor)
            {
                if(cursors.ContainsKey(mouseCursor)) return cursors[mouseCursor];
                object cursor = new Cursor();
                typeof(Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cursor, (int)mouseCursor);
                return cursors[mouseCursor] = (Cursor)cursor;
            }

            internal MDLabel(string label, MDType type, bool enableSpace)
            {
                SetFont(style);
                text = label;
                enableRichText = true;
                focusable = true;
                selection.isSelectable = true;
                style.cursor = GetDefaultCursor(MouseCursor.Text);

                style.whiteSpace = WhiteSpace.Normal;
                if(enableSpace) style.marginTop = 12;
                switch(type)
                {
                    case MDType.h1:
                        style.fontSize = FONT_SIZE * 2.0f;
                        style.unityFontStyleAndWeight = FontStyle.Bold;
                        style.borderBottomColor = new Color(0.5f,0.5f,0.5f,0.5f);
                        style.borderBottomWidth = 1;
                        break;
                    case MDType.h2:
                        style.fontSize = FONT_SIZE * 1.5f;
                        style.unityFontStyleAndWeight = FontStyle.Bold;
                        style.borderBottomColor = new Color(0.5f,0.5f,0.5f,0.5f);
                        style.borderBottomWidth = 1;
                        break;
                    case MDType.h3:
                        style.fontSize = FONT_SIZE * 1.25f;
                        style.unityFontStyleAndWeight = FontStyle.Bold;
                        break;
                    case MDType.h4:
                    case MDType.h5:
                    case MDType.h6:
                        style.fontSize = FONT_SIZE;
                        style.unityFontStyleAndWeight = FontStyle.Bold;
                        break;
                    default:
                        style.fontSize = FONT_SIZE;
                        break;
                }
            }

            internal MDLabel(string label)
            {
                SetFont(style);
                text = label;
                enableRichText = true;
                style.fontSize = FONT_SIZE;
            }
        }

        // リスト用にマーカーとラベルを表示
        internal class MDList : VisualElement
        {
            private MDLabel veMarker;
            private MDLabel veLabel;
            internal MDList(string label, string marker, bool enableSpace, int depth)
            {
                // マーカーとラベルを横並びに
                style.flexDirection = FlexDirection.Row;
                style.marginLeft = depth * 24;
                if(enableSpace) style.marginTop = 12;

                // マーカーは右揃えにして整った見た目になるように
                veMarker = new MDLabel(marker);
                veMarker.style.unityTextAlign = TextAnchor.UpperRight;
                veMarker.style.width = 24;
                veMarker.style.paddingTop = veMarker.style.paddingTop.value.value + 1;
                Add(veMarker);

                // ラベルは通常同様に表示
                veLabel = new MDLabel(label, MDType.p, false);
                veLabel.style.paddingRight = veLabel.style.paddingRight.value.value + 20;
                veLabel.style.flexGrow = 1;
                Add(veLabel);
            }
        }

        // 引用
        internal class Blockquote : Box
        {
            internal Blockquote(bool enableSpace)
            {
                style.backgroundColor = new Color(0.5f,0.5f,0.5f,0.1f);
                style.paddingLeft = 8;
                style.borderLeftColor = new Color(0.196f,0.352f,0.592f,1);
                style.borderLeftWidth = 2;
                style.borderTopWidth = 0;
                style.borderBottomWidth = 0;
                style.borderRightWidth = 0;
                if(enableSpace)
                {
                    style.marginTop = 12;
                }
            }
        }

        // 描画
        internal static VisualElement Draw(string markdown)
        {
            var root = new VisualElement();
            var mds = Get(markdown); // Markdownを要素ごとに分解
            var prevtype = MDType.p; // 直前の要素の種類
            int prevDepth = 0; // 多重引用の深さ
            var listCounts = new Dictionary<int,int>(); // 連番リストの深さと番号

            foreach(var mdpart in mds)
            {
                bool enableSpace = true;

                // 通常のテキストやリストの場合は改行でスペースを開けない
                switch(mdpart.Item1)
                {
                    case MDType.p:
                    case MDType.ul:
                    case MDType.ol:
                        enableSpace = prevtype != mdpart.Item1;
                        break;
                }

                if(mdpart.Item3 > prevDepth)
                {
                    // 引用の深度が直前より大きい場合は子にBlockquoteを追加してそれを親にする
                    enableSpace = true;
                    for(int i = 0; i < mdpart.Item3 - prevDepth; i++)
                    {
                        var b = new Blockquote(enableSpace);
                        root.Add(b);
                        root = b;
                        enableSpace = false;
                    }
                }
                else if(mdpart.Item3 < prevDepth)
                {
                    // 引用の深度が直前より浅い場合は親に戻る
                    for(int i = 0; i < prevDepth - mdpart.Item3; i++)
                    {
                        root = root.parent;
                    }
                    enableSpace = true;
                }

                // 要素の追加
                switch(mdpart.Item1)
                {
                    case MDType.ul:
                        root.Add(new MDList(mdpart.Item2, "・ ", enableSpace, mdpart.Item4));
                        break;
                    case MDType.ol:
                        int count = 1;
                        if(listCounts.ContainsKey(mdpart.Item4))
                        {
                            count = ++listCounts[mdpart.Item4];
                        }
                        else
                        {
                            listCounts[0] = 1;
                        }
                        root.Add(new MDList(mdpart.Item2, $"{count}. ", enableSpace, mdpart.Item4));
                        break;
                    default:
                        if(prevtype == MDType.ol) listCounts.Clear();
                        root.Add(new MDLabel(mdpart.Item2, mdpart.Item1, enableSpace));
                        break;
                }
                prevtype = mdpart.Item1;
                prevDepth = mdpart.Item3;
            }
            return root;
        }

        // Markdownを要素で分解する関数
        private static List<(MDType,string,int,int)> Get(string markdown)
        {
            if(mdlist.ContainsKey(markdown)) return mdlist[markdown];

            var temp = new List<(MDType,string,int,int)>();
            int prevDepth = 0; // 多重引用の深さ
            bool isNewline = false;
            int listSpace = 1; // listのインデント

            var sr = new StringReader(markdown);
            var sb = new StringBuilder();
            string line;

            while((line = sr.ReadLine()) != null)
            {
                var mdpart = CheckType(line);

                // 通常のテキストである場合の処理
                if(mdpart.type == MDType.p)
                {
                    // 引用の深さが異なる場合は要素を分割
                    if(prevDepth != mdpart.blockquoteDepth) isNewline = temp.AddFixed((MDType.p,sb,prevDepth,0));

                    // 行頭でない場合は改行時に空白を入れる
                    if(!isNewline) sb.Append(" ");
                    sb.Append(mdpart.text);
                    isNewline = false;

                    // 末尾がスペース2つ以上で終わる場合かつ既存テキストが空白でない場合は改行
                    if(mdpart.isBr && sb.Length > 0)
                    {
                        sb.AppendLine();
                        isNewline = true;
                    }
                }

                // リストである場合の処理
                else if(mdpart.type == MDType.ul || mdpart.type == MDType.ol)
                {
                    isNewline = temp.AddFixed((MDType.p,sb,prevDepth,0));
                    if(listSpace == 1 && mdpart.listDepth > 1) listSpace = mdpart.listDepth;
                    temp.AddFixed((mdpart.type, mdpart.text, mdpart.blockquoteDepth, mdpart.listDepth / listSpace));
                }

                // 改行である場合の処理
                else if(mdpart.type == MDType.br)
                {
                    // 引用の深さが異なる場合は要素を分割
                    if(prevDepth != mdpart.blockquoteDepth) isNewline = temp.AddFixed((MDType.p,sb,prevDepth,0));

                    // そうでない場合かつ既存テキストが空白でない場合は改行
                    else if(sb.Length > 0)
                    {
                        sb.AppendLine();
                        isNewline = true;
                    }
                }

                // ヘッダー用のラインである場合の処理
                else if(mdpart.type == MDType.h1line || mdpart.type == MDType.h2line)
                {
                    if(mdpart.type == MDType.h1line) isNewline = temp.AddFixed((MDType.h1,sb,prevDepth,0));
                    if(mdpart.type == MDType.h2line) isNewline = temp.AddFixed((MDType.h2,sb,prevDepth,0));
                }

                // それ以外の処理
                else
                {
                    // 既存テキストを追加
                    isNewline = temp.AddFixed((MDType.p,sb,prevDepth,0));

                    // 現在の要素を追加
                    temp.AddFixed((mdpart.type, mdpart.text, mdpart.blockquoteDepth, 0));
                }

                // リスト出ない場合はインデントを戻す
                if(mdpart.type != MDType.ul && mdpart.type != MDType.ol) listSpace = 1;
                prevDepth = mdpart.blockquoteDepth;
            }

            temp.AddFixed((MDType.p,sb,prevDepth,0));
            return mdlist[markdown] = temp;
        }

        // StringBuilderを空にしつつ処理
        private static bool AddFixed(this List<(MDType, string, int, int)> list, (MDType, StringBuilder, int, int) item)
        {
            if(item.Item2.Length == 0) return false;
            var s = item.Item2.ToString();
            item.Item2.Clear();
            list.AddFixed((item.Item1, s, item.Item3, item.Item4));
            return true;
        }

        private static void AddFixed(this List<(MDType, string, int, int)> list, (MDType, string, int, int) item)
        {
            var s = item.Item2.Trim();

            // マルチバイト文字を含む場合はスペースを置き換えて変な改行をされないように
            bool hasMultiByte = s.Length != encSjis.GetByteCount(s);
            if(hasMultiByte) s = s.Replace(" ", "\u00A0");

            // マークダウン構文をHTML構文に置換
            s = s.Replace("<br>", System.Environment.NewLine);
            s = s.Replace("``", string.Empty);
            ReplaceSyntax(ref s, "**", "<b>", "</b>");
            ReplaceSyntax(ref s, "__", "<b>", "</b>");
            ReplaceSyntax(ref s, "*", "<i>", "</i>");
            ReplaceSyntax(ref s, "_", "<i>", "</i>");
            if(hasMultiByte) ReplaceSyntax(ref s, "`", "\u00A0<color=#2d9c63ff>", "</color>\u00A0");
            else ReplaceSyntax(ref s, "`", " <color=#2d9c63ff>", "</color> ");
            ReplaceMDLinks(ref s);
            ReplaceLinks(ref s);

            list.Add((item.Item1,s,item.Item3,item.Item4));
        }

        // 構文の置換
        private static void ReplaceSyntax(ref string s, string syntax, string start, string end)
        {
            while(true)
            {
                var first = s.IndexOf(syntax);
                if(first == -1) return;

                var length = syntax.Length;
                var second = s.IndexOf(syntax, first + length);
                if(second == -1) return;

                s = s.Remove(first) + start + s.Substring(first + length);
                var second2 = s.IndexOf(syntax);
                s = s.Remove(second2) + end + s.Substring(second2 + length);
            }
        }

        private static void ReplaceMDLinks(ref string s)
        {
            s = Regex.Replace(s, @"\[([^\]]+)\]\(([^)]+)\)", m =>  $"<a href=\"{m.Groups[2].Value}\">{m.Groups[1].Value}</a>");
        }

        private static void ReplaceLinks(ref string s)
        {
            s = Regex.Replace(s, @"(?<!<a href="")https?://[^\s\n\\\(\)\^\[\]`<>#""%（）{}|]*", m =>  $"<a href=\"{m.Value}\">{m.Value}</a>");
        }

        // 行頭から要素の種類を判定
        private static MDPart CheckType(string line)
        {
            bool isBr = line.EndsWith("  ");
            var trim = line.Trim();
            var depth = GetBlockquoteDepth(ref trim);

            if(trim.StartsWith("###### "))
                return new MDPart(MDType.h6, trim.Substring(7), isBr, depth);
            if(trim.StartsWith("##### "))
                return new MDPart(MDType.h5, trim.Substring(6), isBr, depth);
            if(trim.StartsWith("#### "))
                return new MDPart(MDType.h4, trim.Substring(5), isBr, depth);
            if(trim.StartsWith("### "))
                return new MDPart(MDType.h3, trim.Substring(4), isBr, depth);
            if(trim.StartsWith("## "))
                return new MDPart(MDType.h2, trim.Substring(3), isBr, depth);
            if(trim.StartsWith("# "))
                return new MDPart(MDType.h1, trim.Substring(2), isBr, depth);
            if(trim.StartsWith("- "))
                return new MDPart(MDType.ul, trim.Substring(2), isBr, depth, GetListDepth(line));
            if(trim.StartsWith("+ "))
                return new MDPart(MDType.ul, trim.Substring(2), isBr, depth, GetListDepth(line));
            if(trim.StartsWith("* "))
                return new MDPart(MDType.ul, trim.Substring(2), isBr, depth, GetListDepth(line));
            if(trim.StartsWith("=") && !trim.Any(c => c != '='))
                return new MDPart(MDType.h1line, "", isBr, depth);
            if(trim.StartsWith("-") && !trim.Any(c => c != '-'))
                return new MDPart(MDType.h2line, "", isBr, depth);
            if(string.IsNullOrEmpty(trim))
                return new MDPart(MDType.br, "", isBr, depth);

            var matchOL = Regex.Match(trim, @"\d+. ");
            if(matchOL.Success && matchOL.Index == 0)
                return new MDPart(MDType.ol, Regex.Replace(trim, @"\d+. ", ""), isBr, depth, GetListDepth(line));

            return new MDPart(MDType.p, trim, isBr, depth);
        }

        private static int GetBlockquoteDepth(ref string line)
        {
            int i = 0;
            while(true)
            {
                if(!line.StartsWith(">")) return i;
                i++;
                line = line.Substring(1).TrimStart();
            }
        }

        private static int GetListDepth(string line)
        {
            return line.Length - line.TrimStart().Length;
        }

        internal struct MDPart
        {
            internal MDType type;
            internal string text;
            internal bool isBr;
            internal int blockquoteDepth;
            internal int listDepth;

            internal MDPart(MDType type, string text, bool isBr, int blockquoteDepth, int listDepth = 0)
            {
                this.type = type;
                this.text = text;
                this.isBr = isBr;
                this.blockquoteDepth = blockquoteDepth;
                this.listDepth = listDepth;
            }
        }
    }
}
