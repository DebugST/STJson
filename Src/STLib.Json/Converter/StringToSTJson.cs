using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ME = STLib.Json.StringToSTJson;

namespace STLib.Json
{
    internal class StringToSTJson
    {
        public static STJson Get(string strJson) {
            var smbs = STJsonParse.GetSymbols(strJson);
            if (smbs.Count == 0) {
                throw new STJsonParseException(0, "Invalid string.");
            }
            int nIndex = 1;
            if (smbs[0].Value == "{") {
                return ME.GetObject(smbs, ref nIndex);
            }

            if (smbs[0].Value == "[") {
                return ME.GetArray(smbs, ref nIndex);
            }
            throw new STJsonParseException(
                smbs[0].Index,
                "Invalid char form index [" + smbs[0].Index + "]{" + smbs[0].Value + "}"
                );
        }

        private static STJson GetObject(List<STJsonParse.Symbol> smbs, ref int nIndex) {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Object);
            if (smbs[nIndex].Value == "}") return json;
            while (nIndex < smbs.Count) {
                var smb = smbs[nIndex++];
                if (smb.Type != STJsonParse.SymbolType.String) {
                    throw new STJsonParseException(
                        smb.Index,
                        "Invalid char form index [" + smb.Index + "]{" + smb.Value + "}"
                        );
                }
                var jv = json.SetKey(smb.Value);
                smb = smbs[nIndex++];
                if (smb.Value != ":") {
                    throw new STJsonParseException(
                        smb.Index,
                        "Invalid char form index [" + smb.Index + "]{" + smb.Value + "}"
                        );
                }
                smb = smbs[nIndex++];
                switch (smb.Type) {
                    case STJsonParse.SymbolType.KeyWord:
                        switch (smb.Value) {
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
                    case STJsonParse.SymbolType.String:
                        jv.SetValue(smb.Value);
                        break;
                    case STJsonParse.SymbolType.Number:
                        jv.SetValue(double.Parse(smb.Value));
                        break;
                    case STJsonParse.SymbolType.ObjectStart:
                        jv.SetValue(ME.GetObject(smbs, ref nIndex));
                        break;
                    case STJsonParse.SymbolType.ArrayStart:
                        jv.SetValue(ME.GetArray(smbs, ref nIndex));
                        break;
                    case STJsonParse.SymbolType.ObjectEnd:
                        return json;
                    default:
                        throw new STJsonParseException(
                        smb.Index,
                        "Invalid char form index [" + smb.Index + "]{" + smb.Value + "}"
                        );
                }
                smb = smbs[nIndex++];
                switch (smb.Value) {
                    case ",": continue;
                    case "}": return json;
                    default:
                        throw new STJsonParseException(
                        smb.Index,
                        "Invalid char form index [" + smb.Index + "]{" + smb.Value + "}"
                        );
                }
            }
            throw new STJsonParseException(-1, "Incomplete string.");
        }

        private static STJson GetArray(List<STJsonParse.Symbol> smbs, ref int nIndex) {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Array);
            while (nIndex < smbs.Count) {
                var smb = smbs[nIndex++];
                switch (smb.Type) {
                    case STJsonParse.SymbolType.String:
                        json.Append(smb.Value);
                        break;
                    case STJsonParse.SymbolType.Number:
                        json.Append(double.Parse(smb.Value));
                        break;
                    case STJsonParse.SymbolType.KeyWord:
                        switch (smb.Value) {
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
                    case STJsonParse.SymbolType.ObjectStart:
                        json.Append(ME.GetObject(smbs, ref nIndex));
                        break;
                    case STJsonParse.SymbolType.ArrayStart:
                        json.Append(ME.GetArray(smbs, ref nIndex));
                        break;
                    case STJsonParse.SymbolType.ArrayEnd:
                        return json;
                    default:
                        throw new STJsonParseException(
                        smb.Index,
                        "Invalid char form index [" + smb.Index + "]{" + smb.Value + "}"
                        );
                }
                smb = smbs[nIndex++];
                switch (smb.Value) {
                    case ",": continue;
                    case "]": return json;
                    default:
                        throw new STJsonParseException(
                        smb.Index,
                        "Invalid char form index [" + smb.Index + "]{" + smb.Value + "}"
                        );
                }
            }
            throw new STJsonParseException(-1, "Incomplete string.");
        }
    }
}

