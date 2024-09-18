using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace jp.lilxyzw.lilycalinventory
{
    internal static class JsonDictionaryParser
    {
        // JsonUtilityではDictionaryを扱えないため自作
        // 言語ファイル読み込み専用なので他の型を読み込もうとすると失敗します
        internal static Dictionary<string, T> Deserialize<T>(string json)
        {
            var sr = new StringReader(json);
            var obj = ParseAnyObject(sr, true);
            return CastDictionary<T>((Dictionary<string, object>)obj);
        }

        private static object ParseAnyObject(StringReader sr, bool isRoot = false)
        {
            int v;
            while((v = sr.ReadToNonSpaseChar()) != -1)
            {
                switch(CheckType(v))
                {
                    case ObjectType.Object: return ParseObject(sr);
                    case ObjectType.Array: throw new FormatException();
                    case ObjectType.String: return ParseString(sr);
                    case ObjectType.True: throw new FormatException();
                    case ObjectType.False: throw new FormatException();
                    case ObjectType.Null: throw new FormatException();
                    default : throw new FormatException();
                }
            }

            if(!isRoot) throw new FormatException();
            return null;
        }

        private static Dictionary<string, T> CastDictionary<T>(Dictionary<string, object> dic)
        {
            var res = new Dictionary<string, T>();
            foreach(var kv in dic)
            {
                res[kv.Key] = (T)kv.Value;
            }
            return res;
        }

        private static Dictionary<string, object> ParseObject(StringReader sr)
        {
            var obj = new Dictionary<string, object>();
            int v;
            while((v = sr.ReadToNonSpaseChar()) != -1)
            {
                if(v == '}') return obj;
                if(v != '"') throw new FormatException();
                var param = ParseParameter(sr);
                obj.Add(param.Item1, param.Item2);

                while((v = sr.ReadToNonSpaseChar()) != -1 && v != ',')
                {
                    if(v == '}') return obj;
                    if(v == ',') break;
                }
            }
            throw new FormatException();
        }

        private static (string, object) ParseParameter(StringReader sr)
        {
            var name = ParseString(sr);

            int v;
            while((v = sr.ReadToNonSpaseChar()) != -1)
            {
                if(v == ':') return (name, ParseAnyObject(sr));
            }

            throw new FormatException();
        }

        private static string ParseString(StringReader sr)
        {
            var sb = new StringBuilder();
            int v;
            while((v = sr.Read()) != -1)
            {
                char c = (char)v;
                if(v == '\\')
                {
                    v = sr.Read();
                    if(v == -1) throw new FormatException();
                    switch(v)
                    {
                        case '\"':
                            c = '\"'; break;
                        case '\\':
                            c = '\\'; break;
                        case '/':
                            c = '/'; break;
                        case 'b':
                            c = '\b'; break;
                        case 'f':
                            c = '\f'; break;
                        case 'n':
                            c = '\n'; break;
                        case 'r':
                            c = '\r'; break;
                        case 't':
                            c = '\t'; break;
                        default:
                            throw new FormatException();
                    }
                }
                else if(v == '"')
                {
                    return sb.ToString();
                }
                sb.Append(c);
            }
            throw new FormatException();
        }

        internal static int ReadToNonSpaseChar(this StringReader sr)
        {
            int v;
            while((v = sr.Read()) != -1 && char.IsWhiteSpace((char)v))
            {
            }
            return v;
        }

        private enum ObjectType
        {
            Object,
            Array,
            Number,
            String,
            True,
            False,
            Null,
            Invalid
        }

        private static ObjectType CheckType(int v)
        {
            return CheckType((char)v);
        }

        private static ObjectType CheckType(char c)
        {
            if(char.IsDigit(c)) return ObjectType.Number;
            switch(c)
            {
                case '{': return ObjectType.Object;
                case '[': return ObjectType.Array;
                case '"': return ObjectType.String;
                case 't': return ObjectType.True;
                case 'f': return ObjectType.False;
                case 'n': return ObjectType.Null;
                default : return ObjectType.Invalid;
            }
        }
    }
}
