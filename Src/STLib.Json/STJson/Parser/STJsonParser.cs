using System;
using System.Text;
using System.Collections.Generic;

using ME = STLib.Json.STJsonParser;

namespace STLib.Json
{
    internal class STJsonParser
    {
        public static STJson Parse(string str_json)
        {
            var lst_token = new STJsonTokenizer(str_json).GetTokens();
            return ME.Parse(lst_token);
        }

        internal static STJson Parse(List<STJsonToken> lst_token)
        {
            if (lst_token.Count == 0) {
                throw new STJsonParseException(-1, "Invalid JSON string.");
            }
            int n_index = 1;
            STJson json = null;
            switch (lst_token[0].Type) {
                case STJsonTokenType.ObjectStart:
                    json = ME.GetObject(lst_token, ref n_index);
                    break;
                case STJsonTokenType.ArrayStart:
                    json = ME.GetArray(lst_token, ref n_index);
                    break;
                default:
                    throw new STJsonParseException(lst_token[0]);
            }
            if (n_index < lst_token.Count) {
                throw new STJsonParseException(lst_token[n_index]);
            }
            return json;
        }

        private static STJson GetObject(List<STJsonToken> lst_token, ref int n_index)
        {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Object);
            while (n_index < lst_token.Count) {
                var token = lst_token[n_index];
                if (token.Type == STJsonTokenType.ObjectEnd) {  // '}'
                    n_index++;
                    return json;
                }
                token = lst_token[n_index++];
                if (token.Type != STJsonTokenType.String && token.Type != STJsonTokenType.Symbol) {
                    throw new STJsonParseException(token);
                }
                var jv = json.SetKey(token.Value);
                if (n_index >= lst_token.Count) throw new STJsonParseException(-1, "Incomplete JSON string.");
                token = lst_token[n_index++];
                if (token.Type != STJsonTokenType.KVSplitor) {  // ':'
                    throw new STJsonParseException(token);
                }
                if (n_index >= lst_token.Count) throw new STJsonParseException(-1, "Incomplete JSON string.");
                token = lst_token[n_index++];
                switch (token.Type) {
                    case STJsonTokenType.Symbol:
                        switch (token.Value) {
                            case "true":
                                jv.SetValue(true);
                                break;
                            case "false":
                                jv.SetValue(false);
                                break;
                            case "null":
                                jv.SetValue(value: null);
                                break;
                            default:
                                throw new STJsonParseException(token);
                        }
                        break;
                    case STJsonTokenType.String:
                        jv.SetValue(ME.ParseString(token));
                        break;
                    case STJsonTokenType.Long:
                        jv.SetValue(ME.ParseNumberLong(token));
                        break;
                    case STJsonTokenType.Double:
                        jv.SetValue(ME.ParseNumberDouble(token));
                        break;
                    case STJsonTokenType.ObjectStart:
                        jv.SetValue(ME.GetObject(lst_token, ref n_index));
                        break;
                    case STJsonTokenType.ArrayStart:
                        jv.SetValue(ME.GetArray(lst_token, ref n_index));
                        break;
                    default:
                        throw new STJsonParseException(token);
                }
                token = lst_token[n_index++];
                switch (token.Type) {
                    case STJsonTokenType.ItemSplitor: continue;     // ','
                    case STJsonTokenType.ObjectEnd: return json;    // '}'
                    default:
                        throw new STJsonParseException(token);
                }
            }
            throw new STJsonParseException(-1, "Incomplete JSON string.");
        }

        private static STJson GetArray(List<STJsonToken> lst_token, ref int n_index)
        {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Array);
            while (n_index < lst_token.Count) {
                var token = lst_token[n_index++];
                switch (token.Type) {
                    case STJsonTokenType.ArrayEnd:
                        return json;
                    case STJsonTokenType.String:
                        json.Append(ME.ParseString(token));
                        break;
                    case STJsonTokenType.Long:
                        json.Append(ME.ParseNumberLong(token));
                        break;
                    case STJsonTokenType.Double:
                        json.Append(ME.ParseNumberDouble(token));
                        break;
                    case STJsonTokenType.Symbol:
                        switch (token.Value) {
                            case "true":
                                json.Append(true);
                                break;
                            case "false":
                                json.Append(false);
                                break;
                            case "null":
                            case "undefined":
                                json.Append(json: null);
                                break;
                            default:
                                throw new STJsonParseException(token);
                        }
                        break;
                    case STJsonTokenType.ObjectStart:
                        json.Append(ME.GetObject(lst_token, ref n_index));
                        break;
                    case STJsonTokenType.ArrayStart:
                        json.Append(ME.GetArray(lst_token, ref n_index));
                        break;
                    default:
                        throw new STJsonParseException(token);
                }
                token = lst_token[n_index++];
                switch (token.Type) {
                    case STJsonTokenType.ItemSplitor: continue; // ','
                    case STJsonTokenType.ArrayEnd: return json; // ']'
                    default:
                        throw new STJsonParseException(token);
                }
            }
            throw new STJsonParseException(-1, "Incomplete JSON string.");
        }

        internal static long ParseNumberLong(STJsonToken token)
        {
            // note: -0xAAA, 0XAAA
            var b_flag = token.Value[0] == '-';
            var str = b_flag ? token.Value.Substring(1) : token.Value;
            try {
                for (int i = 1; i < str.Length; i++) {
                    if (i > 2) break;
                    var ch = token.Value[i];
                    switch (ch) {
                        case 'x':
                        case 'X':   // -0x** will get a exception.
                            return b_flag ? 0 - Convert.ToInt64(str, 16) : Convert.ToInt64(str, 16);
                    }
                }
                return b_flag ? 0 - Convert.ToInt64(str) : Convert.ToInt64(str);
            } catch {
                throw new STJsonParseException(token);
            }
        }

        internal static double ParseNumberDouble(STJsonToken token)
        {
            try {
                return Convert.ToDouble(token.Value);
            } catch {
                throw new STJsonParseException(token);
            }
        }

        internal static string ParseString(STJsonToken token)
        {
            int n_hex_string = 0;
            string str_temp = string.Empty;
            StringBuilder sb = new StringBuilder();

            for (int i = 0, n_len = token.Value.Length; i < n_len; i++) {
                var ch = token.Value[i];
                if (ch != '\\') {
                    sb.Append(ch);
                    continue;
                }
                if (++i >= n_len) {
                    throw new STJsonParseException(token);
                }
                ch = token.Value[i];
                var b_is_newline = false;
                switch (ch) {
                    case '\r':
                        for (i++; i < n_len; i++) {
                            switch (token.Value[i]) {
                                case '\r': continue;
                                case '\n': b_is_newline = true; break;
                                default:
                                    throw new STJsonParseException(token);
                            }
                            if (b_is_newline) break;
                        }
                        continue;
                    case '\n': continue;
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
                        n_hex_string = ch == 'x' ? 2 : 4;
                        if (i + n_hex_string >= token.Value.Length) {
                            throw new STJsonParseException(token);
                        }
                        str_temp = token.Value.Substring(i + 1, n_hex_string);
                        try {
                            sb.Append((char)Convert.ToUInt16(str_temp, 16));
                        } catch {
                            throw new STJsonParseException(token);
                        }
                        i += n_hex_string;
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
