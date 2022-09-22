using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace STLib.Json
{
    internal class STJsonParse
    {
        public enum SymbolType
        {
            KeyWord, Number, String, ItemSplitor, KVSplitor, ObjectStart, ObjectEnd, ArrayStart, ArrayEnd
        }

        public struct Symbol
        {
            public int Index;
            public string Value;
            public SymbolType Type;

            public Symbol(int nIndex, string strValue, SymbolType type) {
                this.Index = nIndex;
                this.Value = strValue;
                this.Type = type;
            }

            public override string ToString() {
                return string.Format("\"{0}\" - [{1}] - [{2}]", this.Value, this.Index, this.Type);
            }
        }

        private static Regex m_reg_escape = new Regex(@"\\.{1,5}");
        private static Dictionary<string, string> m_dic_char_hex = new Dictionary<string, string>();

        public STJsonParse() {
            for (int i = 0; i <= 0xFFFF; i++) {
                if (i <= 0xFF) {
                    m_dic_char_hex.Add(i.ToString("X").PadLeft(2, '0'), ((char)i).ToString());
                }
                m_dic_char_hex.Add(i.ToString("X").PadLeft(4, '0'), ((char)i).ToString());
            }
        }

        public static List<Symbol> GetSymbols(string strJson) {
            Symbol smb = new Symbol();
            List<Symbol> lst = new List<Symbol>();
            Stack<char> stack_region = new Stack<char>();
            for (int i = 0; i < strJson.Length; i++) {
                var c = strJson[i];
                if (('0' <= c && c <= '9') || c == '-') {
                    smb = STJsonParse.GetNumber(strJson, i);
                    lst.Add(smb);
                    i += smb.Value.Length - 1;
                    continue;
                }
                switch (c) {
                    case '{':   // object start
                        stack_region.Push('{');
                        lst.Add(new Symbol(i, "{", SymbolType.ObjectStart));
                        continue;
                    case '[':   // array start
                        stack_region.Push('[');
                        lst.Add(new Symbol(i, "[", SymbolType.ArrayStart));
                        continue;
                    case ',':   // item splitor
                        lst.Add(new Symbol(i, ",", SymbolType.ItemSplitor));
                        continue;
                    case ':':   // key value splitor
                        lst.Add(new Symbol(i, ":", SymbolType.KVSplitor));
                        continue;
                    case ']':   // array end
                        if (stack_region.Count == 0 || stack_region.Pop() != '[') {
                            throw new STJsonParseException(i, "Invalid char, missing '[' or '}'. Index: " + i);
                        }
                        lst.Add(new Symbol(i, "]", SymbolType.ArrayEnd));
                        continue;
                    case '}':   // object end
                        if (stack_region.Count == 0 || stack_region.Pop() != '{') {
                            throw new STJsonParseException(i, "Invalid char, missing '[' or '}'. Index: " + i);
                        }
                        lst.Add(new Symbol(i, "}", SymbolType.ObjectEnd));
                        continue;
                    case 't':   // [keyword] - true
                        lst.Add(STJsonParse.GetKeyWord(strJson, i, "true"));
                        i += 3;
                        continue;
                    case 'f':   // [keyword] - false
                        lst.Add(STJsonParse.GetKeyWord(strJson, i, "false"));
                        i += 4;
                        continue;
                    case 'n':   // [keyword] - null
                        lst.Add(STJsonParse.GetKeyWord(strJson, i, "null"));
                        i += 3;
                        continue;
                    case '"':   // string
                        smb = STJsonParse.GetString(strJson, i);
                        i += smb.Value.Length + 1;
                        smb.Value = STJsonParse.ParseString(smb);
                        lst.Add(smb);
                        continue;
                    case ' ':
                    case '\r':  // space
                    case '\n':
                    case '\t':
                        continue;
                    default:
                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
                }
            }
            return lst;
        }

        private static Symbol GetNumber(string strText, int nIndex) {
            bool bDot = false;
            Symbol smb = new Symbol() {
                Index = nIndex,
                Type = SymbolType.Number
            };
            for (int i = nIndex; i < strText.Length; i++) {
                var c = strText[i];
                if (c == '-') {
                    if (i != nIndex) {
                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
                    }
                    continue;
                }
                if (c == '.') {
                    if (bDot || i == nIndex || (strText[nIndex] == '-' && i == nIndex + 1)) {
                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
                    }
                    bDot = true;
                    continue;
                }
                if ('0' <= c && c <= '9') continue;
                smb.Value = strText.Substring(nIndex, i - nIndex);
                return smb;
            }
            throw new STJsonParseException(-1, "Incomplete string.");
        }

        private static Symbol GetKeyWord(string strText, int nIndex, string strKeyWord) {
            Symbol smb = new Symbol() {
                Index = nIndex,
                Type = SymbolType.KeyWord
            };
            if (strText.Substring(nIndex, strKeyWord.Length) == strKeyWord) {
                smb.Value = strKeyWord;
                return smb;
            }
            throw new STJsonParseException(nIndex, "Invalid char. Index: " + nIndex);
        }

        private static Symbol GetString(string strText, int nIndex) {
            char ch_last = '\0';
            Symbol smb = new Symbol() {
                Index = nIndex,
                Type = SymbolType.String
            };
            if (strText[nIndex] != '"') {
                return smb;
            }
            for (int i = nIndex + 1; i < strText.Length; i++) {
                var ch = strText[i];
                if (ch == '"' && ch_last != '\\') {
                    smb.Value = strText.Substring(nIndex + 1, i - nIndex - 1);
                    return smb;
                }
                ch_last = ch;
            }
            throw new STJsonParseException(nIndex, "Can not get a string. Index: " + nIndex);
        }

        private static string ParseString(Symbol smb) {
            string strText = smb.Value;
            string strHex = string.Empty;
            return m_reg_escape.Replace(strText, (m) => {
                switch (m.Value[1]) {
                    case 't':
                        return "\t" + m.Value.Substring(2);
                    case 'r':
                        return "\r" + m.Value.Substring(2);
                    case 'n':
                        return "\n" + m.Value.Substring(2);
                    case 'x':
                        if (m.Length >= 4) {
                            strHex = m.Value.Substring(2, 2).ToUpper();
                            if (!m_dic_char_hex.ContainsKey(strHex)) {
                                throw new STJsonCastException(
                                    "Can not parse the hex string [" + m.Value + "] from string [" + strText + "]." +
                                    " Index: " + smb.Index);
                            }
                            return m_dic_char_hex[strHex] + m.Value.Substring(4);
                        }
                        break;
                    case 'u':
                        if (m.Length == 6) {
                            strHex = m.Value.Substring(2, 2).ToUpper();
                            if (!m_dic_char_hex.ContainsKey(strHex)) {
                                throw new STJsonCastException(
                                    "Can not parse the unicode string [" + m.Value + "] from string [" + strText + "]." +
                                    " Index: " + smb.Index);
                            }
                            return m_dic_char_hex[strHex];
                        }
                        break;
                }
                return m.Value.Substring(1);
            });
        }
    }
}

