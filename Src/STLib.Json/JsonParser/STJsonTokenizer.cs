using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using ME = STLib.Json.STJsonTokenizer;

namespace STLib.Json
{
    internal class STJsonTokenizer
    {
        public enum TokenType
        {
            KeyWord, Long, Double, String, ItemSplitor, KVSplitor, ObjectStart, ObjectEnd, ArrayStart, ArrayEnd
        }

        public struct Token
        {
            public int Index;
            public string Value;
            public TokenType Type;

            public Token(int nIndex, string strValue, TokenType type) {
                this.Index = nIndex;
                this.Value = strValue;
                this.Type = type;
            }

            public override string ToString() {
                return string.Format("\"{0}\" - [{1}] - [{2}]", this.Value, this.Index, this.Type);
            }
        }

        private static Dictionary<string, string> m_dic_char_hex = new Dictionary<string, string>();

        static STJsonTokenizer() {
            for (int i = 0; i <= 0xFFFF; i++) {
                if (i <= 0xFF) {
                    m_dic_char_hex.Add(i.ToString("X").PadLeft(2, '0'), ((char)i).ToString());
                }
                m_dic_char_hex.Add(i.ToString("X").PadLeft(4, '0'), ((char)i).ToString());
            }
        }

        public static List<Token> GetTokens(string strJson) {
            Token token = new Token();
            List<Token> lst = new List<Token>();
            Stack<char> stack_region = new Stack<char>();
            for (int i = 0; i < strJson.Length; i++) {
                var c = strJson[i];
                if (('0' <= c && c <= '9') || c == '-') {
                    token = ME.GetNumber(strJson, i);
                    lst.Add(token);
                    i += token.Value.Length - 1;
                    continue;
                }
                switch (c) {
                    case '{':   // object start
                        stack_region.Push('{');
                        lst.Add(new Token(i, "{", TokenType.ObjectStart));
                        continue;
                    case '[':   // array start
                        stack_region.Push('[');
                        lst.Add(new Token(i, "[", TokenType.ArrayStart));
                        continue;
                    case ',':   // item splitor
                        lst.Add(new Token(i, ",", TokenType.ItemSplitor));
                        continue;
                    case ':':   // key value splitor
                        lst.Add(new Token(i, ":", TokenType.KVSplitor));
                        continue;
                    case ']':   // array end
                        if (stack_region.Count == 0 || stack_region.Pop() != '[') {
                            throw new STJsonParseException(i, "Invalid char, missing '[' or '}'. Index: " + i);
                        }
                        lst.Add(new Token(i, "]", TokenType.ArrayEnd));
                        continue;
                    case '}':   // object end
                        if (stack_region.Count == 0 || stack_region.Pop() != '{') {
                            throw new STJsonParseException(i, "Invalid char, missing '[' or '}'. Index: " + i);
                        }
                        lst.Add(new Token(i, "}", TokenType.ObjectEnd));
                        continue;
                    case 't':   // [keyword] - true
                        lst.Add(ME.GetKeyWord(strJson, i, "true"));
                        i += 3;
                        continue;
                    case 'f':   // [keyword] - false
                        lst.Add(ME.GetKeyWord(strJson, i, "false"));
                        i += 4;
                        continue;
                    case 'n':   // [keyword] - null
                        lst.Add(ME.GetKeyWord(strJson, i, "null"));
                        i += 3;
                        continue;
                    case '"':   // string
                        token = ME.GetString(strJson, i);
                        i += token.Value.Length + 1;
                        token.Value = ME.ParseString(token);
                        lst.Add(token);
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

        private static Token GetNumber(string strText, int nIndex) {
            bool bDot = false, bE = false;
            Token token = new Token() {
                Index = nIndex,
                Type = TokenType.Long
            };
            for (int i = nIndex; i < strText.Length; i++) {
                var c = strText[i];
                if (c == '-') {                 // -123
                    if (i != nIndex) {
                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
                    }
                    continue;
                }
                if (c == '.') {                 // 1.23
                    if (bDot || bE || i == nIndex || strText[i - 1] == '-') {
                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
                    }
                    bDot = true;
                    token.Type = TokenType.Double;
                    continue;
                }
                if (c == 'E' || c == 'e') {     // 12E+3 Regex:-?\d+(\.\d+)?E[+-]?(\d+)
                    if (bE || i == nIndex || strText[i - 1] == '-' || strText[i - 1] == '.') {
                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
                    }
                    if (i + 2 >= strText.Length) {
                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
                    }
                    c = strText[++i];
                    if (c == '-' || c == '+') { // E+ | E-
                        c = strText[++i];
                    }
                    //if (strText[++i] != '+') { // E+
                    //    throw new STJsonParseException(i + 1, "Invalid char. Index: " + (i + 1));
                    //}
                    bE = true;
                    //c = strText[++i];
                    if (c < '0' && '9' < c) {   // E+[0-9]
                        throw new STJsonParseException(i + 2, "Invalid char. Index: " + (i + 2));
                    }
                    token.Type = TokenType.Double;
                    continue;
                }
                if ('0' <= c && c <= '9') continue;
                token.Value = strText.Substring(nIndex, i - nIndex);
                return token;
            }
            token.Value = strText.Substring(nIndex);
            return token;
        }

        private static Token GetKeyWord(string strText, int nIndex, string strKeyWord) {
            Token token = new Token() {
                Index = nIndex,
                Type = TokenType.KeyWord
            };
            if (strText.Substring(nIndex, strKeyWord.Length) == strKeyWord) {
                token.Value = strKeyWord;
                return token;
            }
            throw new STJsonParseException(nIndex, "Invalid char. Index: " + nIndex);
        }

        private static Token GetString(string strText, int nIndex) {
            //char ch_last = '\0';
            Token token = new Token() {
                Index = nIndex,
                Type = TokenType.String
            };
            if (strText[nIndex] != '"') {
                return token;
            }
            for (int i = nIndex + 1; i < strText.Length; i++) {
                var ch = strText[i];
                if (ch == '\\') {
                    i++;
                    continue;
                }
                if (ch == '"') {
                    token.Value = strText.Substring(nIndex + 1, i - nIndex - 1);
                    return token;
                }
                //if (ch_last == '\\') ch_last = '\0';
                //if (ch == '"' && ch_last != '\\') {
                //    token.Value = strText.Substring(nIndex + 1, i - nIndex - 1);
                //    return token;
                //}
                //ch_last = ch;
            }
            throw new STJsonParseException(nIndex, "Can not get a string. Index: " + nIndex);
        }

        private static string ParseString(Token token) {
            int nHexLen = 0;
            string strTemp = string.Empty;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < token.Value.Length; i++) {
                var ch = token.Value[i];
                if (ch != '\\') {
                    sb.Append(ch);
                    continue;
                }
                i++;
                if (i >= token.Value.Length) {
                    throw new STJsonParseException(token.Index + i, ch);
                }
                ch = token.Value[i];
                switch (ch) {
                    case 'r': sb.Append('\r'); continue;
                    case 'n': sb.Append('\n'); continue;
                    case 't': sb.Append('\t'); continue;
                    case 'f': sb.Append('\f'); continue;
                    case 'b': sb.Append('\b'); continue;
                    case 'a': sb.Append('\a'); continue;
                    case 'v': sb.Append('\v'); continue;
                    case '0': sb.Append('\0'); continue;
                    case 'x':
                    case 'u':
                        nHexLen = ch == 'x' ? 2 : 4;
                        if (i + nHexLen >= token.Value.Length) {
                            throw new STJsonParseException(token.Index + i, ch);
                        }
                        strTemp = token.Value.Substring(i + 1, nHexLen).ToUpper();
                        if (!m_dic_char_hex.ContainsKey(strTemp)) {
                            throw new STJsonParseException(token.Index + i + 1, strTemp[0]);
                        }
                        sb.Append(m_dic_char_hex[strTemp]);
                        i += nHexLen;
                        continue;
                    default:
                        sb.Append(ch);
                        continue;
                }
            }
            return sb.ToString();
        }
    }
}

