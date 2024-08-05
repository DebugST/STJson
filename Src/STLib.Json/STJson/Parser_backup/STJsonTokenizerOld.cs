//using System;
//using System.Text;
//using System.Collections.Generic;

//using ME = STLib.Json.STJsonTokenizerOld;

//namespace STLib.Json
//{
//    internal class STJsonTokenizerOld
//    {
//        public static List<STJsonToken> GetTokens(string str_json)
//        {
//            var token = new STJsonToken();
//            var lst_token = new List<STJsonToken>();
//            Stack<char> stack_region = new Stack<char>();
//            for (int i = 0; i < str_json.Length; i++) {
//                var c = str_json[i];
//                if (('0' <= c && c <= '9') || c == '-') {
//                    token = ME.GetNumber(str_json, i);
//                    lst_token.Add(token);
//                    i += token.Value.Length - 1;
//                    continue;
//                }
//                switch (c) {
//                    case '{':   // object start
//                        stack_region.Push('{');
//                        lst_token.Add(new STJsonToken(i, "{", STJsonTokenType.ObjectStart));
//                        continue;
//                    case '[':   // array start
//                        stack_region.Push('[');
//                        lst_token.Add(new STJsonToken(i, "[", STJsonTokenType.ArrayStart));
//                        continue;
//                    case ',':   // item splitor
//                        lst_token.Add(new STJsonToken(i, ",", STJsonTokenType.ItemSplitor));
//                        continue;
//                    case ':':   // key value splitor
//                        lst_token.Add(new STJsonToken(i, ":", STJsonTokenType.KVSplitor));
//                        continue;
//                    case ']':   // array end
//                        if (stack_region.Count == 0 || stack_region.Pop() != '[') {
//                            throw new STJsonParseException(i, "Invalid char, missing '[' or '}'. Index: " + i);
//                        }
//                        lst_token.Add(new STJsonToken(i, "]", STJsonTokenType.ArrayEnd));
//                        continue;
//                    case '}':   // object end
//                        if (stack_region.Count == 0 || stack_region.Pop() != '{') {
//                            throw new STJsonParseException(i, "Invalid char, missing '[' or '}'. Index: " + i);
//                        }
//                        lst_token.Add(new STJsonToken(i, "}", STJsonTokenType.ObjectEnd));
//                        continue;
//                    case 't':   // [keyword] - true
//                        lst_token.Add(ME.GetKeyword(str_json, i, "true"));
//                        i += 3;
//                        continue;
//                    case 'f':   // [keyword] - false
//                        lst_token.Add(ME.GetKeyword(str_json, i, "false"));
//                        i += 4;
//                        continue;
//                    case 'n':   // [keyword] - null
//                        lst_token.Add(ME.GetKeyword(str_json, i, "null"));
//                        i += 3;
//                        continue;
//                    case '"':   // string
//                        token = ME.GetString(str_json, i);
//                        i += token.Value.Length + 1;
//                        lst_token.Add(token);
//                        continue;
//                    case ' ':
//                    case '\r':  // space
//                    case '\n':
//                    case '\t':
//                        continue;
//                    default:
//                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
//                }
//            }
//            return lst_token;
//        }

//        private static STJsonToken GetNumber(string str_text, int n_index)
//        {
//            bool b_float = false;
//            int n_len = -1;
//            for (int i = 0; i < str_text.Length; i++) {
//                var ch = str_text[i];
//                switch (ch) {
//                    case '+':
//                    case '-':
//                        continue;
//                    case '.':
//                    case 'E':
//                    case 'e':
//                        b_float = true;
//                        continue;
//                }
//                if ('0' <= ch && ch <= '9') continue;
//                break;
//            }
//            return new STJsonToken()
//            {
//                Index = n_index,
//                Type = b_float ? STJsonTokenType.Double : STJsonTokenType.Long,
//                Value = n_len == -1 ? str_text.Substring(n_index) : str_text.Substring(n_index, n_len)
//            };
//        }

//        /*private static STJsonToken GetNumber(string strText, int nIndex)
//        {
//            bool bDot = false, bE = false;
//            var token = new STJsonToken()
//            {
//                Index = nIndex,
//                Type = STJsonTokenType.Long
//            };
//            for (int i = nIndex; i < strText.Length; i++) {
//                var c = strText[i];
//                if (c == '-') {                 // -123
//                    if (i != nIndex) {
//                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
//                    }
//                    continue;
//                }
//                if (c == '.') {                 // 1.23
//                    if (bDot || bE || i == nIndex || strText[i - 1] == '-') {
//                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
//                    }
//                    bDot = true;
//                    token.Type = STJsonTokenType.Double;
//                    continue;
//                }
//                if (c == 'E' || c == 'e') {     // 12E+3 Regex:-?\d+(\.\d+)?E[+-]?(\d+)
//                    if (bE || i == nIndex || strText[i - 1] == '-' || strText[i - 1] == '.') {
//                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
//                    }
//                    if (i + 2 >= strText.Length) {
//                        throw new STJsonParseException(i, "Invalid char. Index: " + i);
//                    }
//                    c = strText[++i];
//                    if (c == '-' || c == '+') { // E+ | E-
//                        c = strText[++i];
//                    }
//                    //if (strText[++i] != '+') { // E+
//                    //    throw new STJsonParseException(i + 1, "Invalid char. Index: " + (i + 1));
//                    //}
//                    bE = true;
//                    //c = strText[++i];
//                    if (c < '0' || '9' < c) {   // E+[0-9]
//                        throw new STJsonParseException(i + 2, "Invalid char. Index: " + (i + 2));
//                    }
//                    token.Type = STJsonTokenType.Double;
//                    continue;
//                }
//                if ('0' <= c && c <= '9') continue;
//                token.Value = strText.Substring(nIndex, i - nIndex);
//                return token;
//            }
//            token.Value = strText.Substring(nIndex);
//            return token;
//        }*/

//        private static STJsonToken GetKeyword(string str_text, int n_index, string str_keyword)
//        {
//            var token = new STJsonToken()
//            {
//                Index = n_index,
//                Type = STJsonTokenType.Keyword
//            };
//            if (str_text.Substring(n_index, str_keyword.Length) == str_keyword) {
//                token.Value = str_keyword;
//                return token;
//            }
//            throw new STJsonParseException(n_index, "Invalid char. Index: " + n_index);
//        }

//        private static STJsonToken GetString(string str_text, int n_index)
//        {
//            var token = new STJsonToken()
//            {
//                Index = n_index,
//                Type = STJsonTokenType.String
//            };
//            if (str_text[n_index] != '"') {
//                return token;
//            }
//            for (int i = n_index + 1; i < str_text.Length; i++) {
//                var ch = str_text[i];
//                switch (ch) {
//                    case '\n':
//                        throw new STJsonParseException(i, "Invalid char(\\n) for a string. Index: " + i);
//                    case '\\':
//                        i++;
//                        continue;
//                    case '"':
//                        token.Value = str_text.Substring(n_index + 1, i - n_index - 1);
//                        return token;
//                }
//            }
//            throw new STJsonParseException(n_index, "Can not get a string. Index: " + n_index);
//        }
//    }
//}