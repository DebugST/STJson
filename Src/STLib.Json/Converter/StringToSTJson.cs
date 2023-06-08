using System;
using System.Collections.Generic;

using ME = STLib.Json.StringToSTJson;

namespace STLib.Json
{
    internal class StringToSTJson
    {
        public static STJson Get(string strJson) {
            var tokens = STJsonTokenizer.GetTokens(strJson);
            if (tokens.Count == 0) {
                throw new STJsonParseException(0, "Invalid string.");
            }
            int nIndex = 1;
            if (tokens[0].Value == "{") {
                return ME.GetObject(tokens, ref nIndex);
            }

            if (tokens[0].Value == "[") {
                return ME.GetArray(tokens, ref nIndex);
            }
            throw new STJsonParseException(
                tokens[0].Index,
                "Invalid char form index [" + tokens[0].Index + "]{" + tokens[0].Value + "}"
                );
        }

        private static STJson GetObject(List<STJsonTokenizer.Token> tokens, ref int nIndex) {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Object);
            if (tokens[nIndex].Value == "}") {
                nIndex++;
                return json;
            }
            while (nIndex < tokens.Count) {
                var token = tokens[nIndex++];
                if (token.Type != STJsonTokenizer.TokenType.String) {
                    throw new STJsonParseException(
                        token.Index,
                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
                        );
                }
                var jv = json.SetKey(token.Value);
                token = tokens[nIndex++];
                if (token.Value != ":") {
                    throw new STJsonParseException(
                        token.Index,
                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
                        );
                }
                token = tokens[nIndex++];
                switch (token.Type) {
                    case STJsonTokenizer.TokenType.KeyWord:
                        switch (token.Value) {
                            case "true":
                                jv.SetValue(true);
                                break;
                            case "false":
                                jv.SetValue(false);
                                break;
                            case "null":
                                jv.SetValue(strText: null);
                                break;
                        }
                        break;
                    case STJsonTokenizer.TokenType.String:
                        jv.SetValue(token.Value);
                        break;
                    case STJsonTokenizer.TokenType.Long:
                        jv.SetValue(Convert.ToInt64(token.Value));
                        break;
                    case STJsonTokenizer.TokenType.Double:
                        jv.SetValue(Convert.ToDouble(token.Value));
                        break;
                    case STJsonTokenizer.TokenType.ObjectStart:
                        jv.SetValue(ME.GetObject(tokens, ref nIndex));
                        break;
                    case STJsonTokenizer.TokenType.ArrayStart:
                        jv.SetValue(ME.GetArray(tokens, ref nIndex));
                        break;
                    case STJsonTokenizer.TokenType.ObjectEnd:
                        return json;
                    default:
                        throw new STJsonParseException(
                        token.Index,
                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
                        );
                }
                token = tokens[nIndex++];
                switch (token.Value) {
                    case ",": continue;
                    case "}": return json;
                    default:
                        throw new STJsonParseException(
                        token.Index,
                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
                        );
                }
            }
            throw new STJsonParseException(-1, "Incomplete string.");
        }

        private static STJson GetArray(List<STJsonTokenizer.Token> smbs, ref int nIndex) {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Array);
            while (nIndex < smbs.Count) {
                var token = smbs[nIndex++];
                switch (token.Type) {
                    case STJsonTokenizer.TokenType.String:
                        json.Append(token.Value);
                        break;
                    case STJsonTokenizer.TokenType.Long:
                        json.Append(Convert.ToInt64(token.Value));
                        break;
                    case STJsonTokenizer.TokenType.Double:
                        json.Append(Convert.ToDouble(token.Value));
                        break;
                    case STJsonTokenizer.TokenType.KeyWord:
                        switch (token.Value) {
                            case "true":
                                json.Append(true);
                                break;
                            case "false":
                                json.Append(false);
                                break;
                            case "null":
                                json.Append(json: null);
                                break;
                        }
                        break;
                    case STJsonTokenizer.TokenType.ObjectStart:
                        json.Append(ME.GetObject(smbs, ref nIndex));
                        break;
                    case STJsonTokenizer.TokenType.ArrayStart:
                        json.Append(ME.GetArray(smbs, ref nIndex));
                        break;
                    case STJsonTokenizer.TokenType.ArrayEnd:
                        return json;
                    default:
                        throw new STJsonParseException(
                        token.Index,
                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
                        );
                }
                token = smbs[nIndex++];
                switch (token.Value) {
                    case ",": continue;
                    case "]": return json;
                    default:
                        throw new STJsonParseException(
                        token.Index,
                        "Invalid char form index [" + token.Index + "]{" + token.Value + "}"
                        );
                }
            }
            throw new STJsonParseException(-1, "Incomplete string.");
        }
    }
}

