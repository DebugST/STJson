//using System;
//using System.Text;
//using System.Collections.Generic;

//using ME = STLib.Json.STJsonParserOld;

//namespace STLib.Json
//{
//    internal class STJsonParserOld
//    {
//        public static STJson Parse(string str_json)
//        {
//            var tokens = STJsonTokenizerOld.GetTokens(str_json);
//            if (tokens.Count == 0) {
//                throw new STJsonParseException(0, "Invalid string.");
//            }
//            int nIndex = 1;
//            if (tokens[0].Value == "{") {
//                return ME.GetObject(tokens, ref nIndex);
//            }

//            if (tokens[0].Value == "[") {
//                return ME.GetArray(tokens, ref nIndex);
//            }
//            throw new STJsonParseException(
//                tokens[0].Index,
//                "Invalid char form index [" + tokens[0].Index + "]{" + tokens[0].Value + "}"
//                );
//        }

//        private static STJson GetObject(List<STJsonToken> lst_token, ref int n_index)
//        {
//            STJson json = new STJson();
//            json.SetModel(STJsonValueType.Object);
//            if (lst_token[n_index].Value == "}") {
//                n_index++;
//                return json;
//            }
//            while (n_index < lst_token.Count) {
//                var token = lst_token[n_index++];
//                if (token.Type != STJsonTokenType.String) {
//                    throw new STJsonParseException(
//                        token.Index,
//                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
//                        );
//                }
//                var jv = json.SetKey(token.Value);
//                token = lst_token[n_index++];
//                if (token.Value != ":") {
//                    throw new STJsonParseException(
//                        token.Index,
//                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
//                        );
//                }
//                token = lst_token[n_index++];
//                switch (token.Type) {
//                    case STJsonTokenType.Keyword:
//                        switch (token.Value) {
//                            case "true":
//                                jv.SetValue(true);
//                                break;
//                            case "false":
//                                jv.SetValue(false);
//                                break;
//                            case "null":
//                                jv.SetValue(value: null);
//                                break;
//                        }
//                        break;
//                    case STJsonTokenType.String:
//                        jv.SetValue(ME.ParseString(token));
//                        break;
//                    case STJsonTokenType.Long:
//                        jv.SetValue(ME.ParseNumberLong(token));
//                        break;
//                    case STJsonTokenType.Double:
//                        jv.SetValue(ME.ParseNumberDouble(token));
//                        break;
//                    case STJsonTokenType.ObjectStart:
//                        jv.SetValue(ME.GetObject(lst_token, ref n_index));
//                        break;
//                    case STJsonTokenType.ArrayStart:
//                        jv.SetValue(ME.GetArray(lst_token, ref n_index));
//                        break;
//                    default:
//                        throw new STJsonParseException(
//                        token.Index,
//                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
//                        );
//                }
//                token = lst_token[n_index++];
//                switch (token.Value) {
//                    case ",": continue;
//                    case "}": return json;
//                    default:
//                        throw new STJsonParseException(
//                        token.Index,
//                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
//                        );
//                }
//            }
//            throw new STJsonParseException(-1, "Incomplete string.");
//        }

//        private static STJson GetArray(List<STJsonToken> lst_token, ref int n_index)
//        {
//            STJson json = new STJson();
//            json.SetModel(STJsonValueType.Array);
//            if (lst_token[n_index].Value == "]") {
//                n_index++;
//                return json;
//            }
//            while (n_index < lst_token.Count) {
//                var token = lst_token[n_index++];
//                switch (token.Type) {
//                    case STJsonTokenType.String:
//                        json.Append(ME.ParseString(token));
//                        break;
//                    case STJsonTokenType.Long:
//                        json.Append(ME.ParseNumberLong(token));
//                        break;
//                    case STJsonTokenType.Double:
//                        json.Append(ME.ParseNumberDouble(token));
//                        break;
//                    case STJsonTokenType.Keyword:
//                        switch (token.Value) {
//                            case "true":
//                                json.Append(true);
//                                break;
//                            case "false":
//                                json.Append(false);
//                                break;
//                            case "null":
//                                json.Append(json: null);
//                                break;
//                        }
//                        break;
//                    case STJsonTokenType.ObjectStart:
//                        json.Append(ME.GetObject(lst_token, ref n_index));
//                        break;
//                    case STJsonTokenType.ArrayStart:
//                        json.Append(ME.GetArray(lst_token, ref n_index));
//                        break;
//                    default:
//                        throw new STJsonParseException(
//                        token.Index,
//                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
//                        );
//                }
//                token = lst_token[n_index++];
//                switch (token.Value) {
//                    case ",": continue;
//                    case "]": return json;
//                    default:
//                        throw new STJsonParseException(
//                        token.Index,
//                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
//                        );
//                }
//            }
//            throw new STJsonParseException(-1, "Incomplete string.");
//        }

//        private static long ParseNumberLong(STJsonToken token)
//        {
//            try {
//                return Convert.ToInt64(token.Value);
//            } catch {
//                throw new STJsonParseException(token.Index, "Can not convert [" + token.Value + "] to long.");
//            }
//        }

//        private static double ParseNumberDouble(STJsonToken token)
//        {
//            try {
//                return Convert.ToDouble(token.Value);
//            } catch {
//                throw new STJsonParseException(token.Index, "Can not convert [" + token.Value + "] to double.");
//            }
//        }

//        private static string ParseString(STJsonToken token)
//        {
//            int n_hex_len = 0;
//            string str_temp = string.Empty;
//            StringBuilder sb = new StringBuilder();
//            for (int i = 0; i < token.Value.Length; i++) {
//                var ch = token.Value[i];
//                if (ch != '\\') {
//                    sb.Append(ch);
//                    continue;
//                }
//                i++;
//                if (i >= token.Value.Length) {
//                    throw new STJsonParseException(token.Index + i, ch);
//                }
//                ch = token.Value[i];
//                switch (ch) {
//                    case 'r': sb.Append('\r'); continue;
//                    case 'n': sb.Append('\n'); continue;
//                    case 't': sb.Append('\t'); continue;
//                    case 'f': sb.Append('\f'); continue;
//                    case 'b': sb.Append('\b'); continue;
//                    case 'a': sb.Append('\a'); continue;
//                    case 'v': sb.Append('\v'); continue;
//                    case '0': sb.Append('\0'); continue;
//                    case 'x':
//                    case 'u':
//                        n_hex_len = ch == 'x' ? 2 : 4;
//                        if (i + n_hex_len >= token.Value.Length) {
//                            throw new STJsonParseException(token.Index + i, ch);
//                        }
//                        str_temp = token.Value.Substring(i + 1, n_hex_len);
//                        try {
//                            sb.Append((char)Convert.ToUInt16(str_temp, 16));
//                        } catch {
//                            throw new STJsonParseException(token.Index + i, "Invalid string [" + (token.Value.Substring(i + str_temp.Length + 2)) + "]");
//                        }
//                        i += n_hex_len;
//                        continue;
//                    default:
//                        sb.Append(ch);
//                        continue;
//                }
//            }
//            return sb.ToString();
//        }
//    }
//}
