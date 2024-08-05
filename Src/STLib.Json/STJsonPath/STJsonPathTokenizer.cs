using System.Collections.Generic;
using System.Text;

using ME = STLib.Json.STJsonPathTokenizer;

namespace STLib.Json
{
    internal class STJsonPathTokenizer
    {
        private static HashSet<char> m_hs_number = new HashSet<char>();
        private static HashSet<char> m_hs_property = new HashSet<char>();
        private static Dictionary<string, string> m_dic_char_hex = new Dictionary<string, string>();

        static STJsonPathTokenizer() {
            for (int i = 0; i <= 0xFFFF; i++) {
                if (i <= 0xFF) {
                    m_dic_char_hex.Add(i.ToString("X").PadLeft(2, '0'), ((char)i).ToString());
                }
                m_dic_char_hex.Add(i.ToString("X").PadLeft(4, '0'), ((char)i).ToString());
            }
            for (char ch = '0'; ch <= '9'; ch++) {
                m_hs_number.Add(ch);
                m_hs_property.Add(ch);
            }
            for (char ch = 'a'; ch <= 'z'; ch++) {
                m_hs_property.Add(ch);
            }
            for (char ch = 'A'; ch <= 'Z'; ch++) {
                m_hs_property.Add(ch);
            }
            m_hs_property.Add('_');
        }

        public static List<STJsonPathToken> GetRange(List<STJsonPathToken> tokens, int nStart, string strLeft, string strRight) {
            int nCounter = 1;
            List<STJsonPathToken> lst_ret = new List<STJsonPathToken>();
            for (int i = nStart + 1; i < tokens.Count; i++) {
                var token = tokens[i];
                if (token.Value == strLeft && token.Type != STJsonPathTokenType.String) {
                    nCounter++;
                } else if (token.Value == strRight && token.Type != STJsonPathTokenType.String) {
                    nCounter--;
                    if (nCounter == 0) {
                        return lst_ret;
                    }
                    if (nCounter < 0) {
                        throw new STJsonPathParseException(i, "Can not match the char [" + token.Value + "]");
                    }
                }
                lst_ret.Add(token);
            }
            throw new STJsonPathParseException(nStart, "Can not match the char [" + tokens[nStart].Value + "]");
        }

        public static List<STJsonPathToken> GetTokens(string strExp) {
            STJsonPathToken token = new STJsonPathToken();
            List<STJsonPathToken> lst = new List<STJsonPathToken>();
            Stack<char> stack_region = new Stack<char>();
            for (int i = 0; i < strExp.Length; i++) {
                var c = strExp[i];
                if (('0' <= c && c <= '9') || c == '-') {
                    token = ME.GetNumber(strExp, i);
                    lst.Add(token);
                    i += token.Value.Length - 1;
                    continue;
                }
                if (c == '_' || ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z')) {
                    token = ME.GetProperty(strExp, i);
                    lst.Add(token);
                    i += token.Value.Length - 1;
                    continue;
                }
                switch (c) {
                    case 't':
                        token = ME.GetKeyWord(strExp, i, "true");
                        i += 3;
                        break;
                    case 'f':
                        token = ME.GetKeyWord(strExp, i, "false");
                        i += 4;
                        break;
                    case 'n':
                        token = ME.GetKeyWord(strExp, i, "null");
                        i += 3;
                        break;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '%':
                    case '^':
                    case '~':
                        token = new STJsonPathToken(i, c.ToString(), STJsonPathTokenType.Operator);
                        break;
                    case '&':
                        if (i + 1 < strExp.Length && strExp[i + 1] == '&') {
                            token = new STJsonPathToken(i++, "&&", STJsonPathTokenType.Operator);
                        } else {
                            token = new STJsonPathToken(i, "&", STJsonPathTokenType.Operator);
                        }
                        break;
                    case '|':
                        if (i + 1 < strExp.Length && strExp[i + 1] == '|') {
                            token = new STJsonPathToken(i++, "||", STJsonPathTokenType.Operator);
                        } else {
                            token = new STJsonPathToken(i, "|", STJsonPathTokenType.Operator);
                        }
                        break;
                    case '!':
                        if (i + 1 < strExp.Length && strExp[i + 1] == '=') {
                            token = new STJsonPathToken(i++, "!=", STJsonPathTokenType.Operator);
                        } else {
                            token = new STJsonPathToken(i, "!", STJsonPathTokenType.Operator);
                        }
                        break;
                    case '=':
                        if (i + 1 < strExp.Length && strExp[i + 1] == '=') {
                            token = new STJsonPathToken(i++, "==", STJsonPathTokenType.Operator);
                        } else {
                            token = new STJsonPathToken(i, "=", STJsonPathTokenType.Operator);
                        }
                        break;
                    case '>':
                        if (i + 1 < strExp.Length) {
                            switch (strExp[i + 1]) {
                                case '>': token = new STJsonPathToken(i++, ">>", STJsonPathTokenType.Operator); break;
                                case '=': token = new STJsonPathToken(i++, ">=", STJsonPathTokenType.Operator); break;
                                default: token = new STJsonPathToken(i, ">", STJsonPathTokenType.Operator); break;
                            }
                        } else {
                            token = new STJsonPathToken(i, ">", STJsonPathTokenType.Operator);
                        }
                        break;
                    case '<':
                        if (i + 1 < strExp.Length) {
                            switch (strExp[i + 1]) {
                                case '<': token = new STJsonPathToken(i++, "<<", STJsonPathTokenType.Operator); break;
                                case '=': token = new STJsonPathToken(i++, "<=", STJsonPathTokenType.Operator); break;
                                default: token = new STJsonPathToken(i, "<", STJsonPathTokenType.Operator); break;
                            }
                        } else {
                            token = new STJsonPathToken(i, "<", STJsonPathTokenType.Operator);
                        }
                        break;
                    case '"':
                    case '\'':
                        token = ME.GetString(strExp, i);
                        i += token.Value.Length + 1;
                        token.Value = ME.ParseString(token);
                        break;
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                        continue;
                    case '$':
                    case '@':
                    case '.':
                    case ',':
                    //case '*':
                    case ':':
                    case '?':
                    case '[':
                    case ']':
                    case '(':
                    case ')':
                        token = new STJsonPathToken(i, c.ToString(), STJsonPathTokenType.None);
                        break;
                    default:
                        throw new STJsonPathParseException(i, "Invalid char. Index: " + i);
                }
                lst.Add(token);
            }
            return lst;
        }

        private static STJsonPathToken GetNumber(string strText, int nIndex) {
            bool bDot = false;
            STJsonPathToken token = new STJsonPathToken() {
                Index = nIndex,
                Type = STJsonPathTokenType.Long
            };
            for (int i = nIndex + (strText[nIndex] == '-' ? 1 : 0); i < strText.Length; i++) {
                var c = strText[i];
                if (c == '.') {
                    if (bDot || i == nIndex || (strText[nIndex] == '-' && i == nIndex + 1)) {
                        throw new STJsonPathParseException(i, "Invalid char. Index: " + i);
                    }
                    bDot = true;
                    token.Type = STJsonPathTokenType.Double;
                    continue;
                }
                if ('0' <= c && c <= '9') continue;
                token.Value = strText.Substring(nIndex, i - nIndex);
                return token;
            }
            token.Value = strText.Substring(nIndex);
            return token;
        }

        private static STJsonPathToken GetKeyWord(string strText, int nIndex, string strKeyWord) {
            STJsonPathToken token = new STJsonPathToken() {
                Index = nIndex,
                Type = STJsonPathTokenType.Keyword
            };
            if (strText.Substring(nIndex, strKeyWord.Length) == strKeyWord) {
                token.Value = strKeyWord;
                return token;
            }
            throw new STJsonPathParseException(nIndex, "Invalid char. Index: " + nIndex);
        }

        private static STJsonPathToken GetProperty(string strText, int nIndex) {
            STJsonPathToken token = new STJsonPathToken() {
                Index = nIndex,
                Type = STJsonPathTokenType.Property
            };
            for (int i = nIndex; i < strText.Length; i++) {
                var c = strText[i];
                if ('0' <= c && c <= '9') continue;
                if ('a' <= c && c <= 'z') continue;
                if ('A' <= c && c <= 'Z') continue;
                if (c == '_') continue;
                token.Value = strText.Substring(nIndex, i - nIndex);
                return token;
            }
            token.Value = strText.Substring(nIndex);
            return token;
        }

        private static STJsonPathToken GetString(string strText, int nIndex) {
            //char ch_last = '\0';
            STJsonPathToken token = new STJsonPathToken() {
                Index = nIndex,
                Type = STJsonPathTokenType.String
            };
            var ch_start = strText[nIndex];
            if (ch_start != '"' && ch_start != '\'') {
                return token;
            }
            for (int i = nIndex + 1; i < strText.Length; i++) {
                var ch = strText[i];
                if (ch == '\\') {
                    i++;
                    continue;
                }
                if (ch == ch_start) {
                    token.Value = strText.Substring(nIndex + 1, i - nIndex - 1);
                    return token;
                }
                //if (ch == ch_start && ch_last != '\\') {
                //    token.Value = strText.Substring(nIndex + 1, i - nIndex - 1);
                //    return token;
                //}
                //ch_last = ch;
            }
            throw new STJsonPathParseException(nIndex, "Can not get a string. Index: " + nIndex);
        }

        private static string ParseString(STJsonPathToken token) {
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
                    throw new STJsonPathParseException(token.Index + i, ch);
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
                            throw new STJsonPathParseException(token.Index + i, ch);
                        }
                        strTemp = token.Value.Substring(i + 1, nHexLen).ToUpper();
                        if (!m_dic_char_hex.ContainsKey(strTemp)) {
                            throw new STJsonPathParseException(token.Index + i + 1, strTemp[0]);
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

