using System;
using System.Collections.Generic;

using ME = STLib.Json.STJsonPathParser;

namespace STLib.Json
{
    internal class STJsonPathParser
    {
        public static List<STJsonPathItem> GetPathItems(List<STJsonPathToken> tokens) {
            var lst = new List<STJsonPathItem>();
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                switch (token.Type) {
                    case STJsonPathTokenType.String:
                    case STJsonPathTokenType.Property:
                    case STJsonPathTokenType.Keyword:
                        lst.Add(new STJsonPathItem(token.Value));
                        continue;
                    case STJsonPathTokenType.Long:
                        lst.Add(new STJsonPathItem(Convert.ToInt32(token.Value))); // this is for index
                        continue;
                }
                switch (token.Value) {
                    case "$":
                        lst.Add(STJsonPathItem.Root);
                        continue;
                    case "@":
                        lst.Add(STJsonPathItem.Current);
                        continue;
                    case "*":
                        lst.Add(STJsonPathItem.Any);
                        continue;
                    case "[":
                        var lst_range = STJsonPathTokenizer.GetRange(tokens, i, "[", "]");
                        if (lst_range.Count < 1) {
                            throw new STJsonPathParseException(i, "The bracket is empty. index: " + i);
                        }
                        lst.Add(ME.GetJsonPathItemFromBracket(lst_range));
                        i += lst_range.Count + 1;
                        continue;
                    case ".":
                        if (i + 1 >= tokens.Count) {
                            throw new STJsonPathParseException(i, "Invalid char [.]. index: " + i);
                        }
                        if (tokens[i + 1].Value == ".") {
                            lst.Add(STJsonPathItem.Depth);
                            i++;
                        }
                        continue;
                }
                throw new STJsonPathParseException(token.Index, "Unknows token [" + token.Value + "]");
            }
            return lst;
        }

        private static STJsonPathItem GetJsonPathItemFromBracket(List<STJsonPathToken> tokens) {
            var token = tokens[0];
            int nLen = tokens.Count;

            if (nLen > 2 && token.IsSymbol("(")) {
                var lst_range = STJsonPathTokenizer.GetRange(tokens, 0, "(", ")");
                if (lst_range.Count < 1) {
                    throw new STJsonPathParseException(token.Index + 1, "The bracket is empty. index: " + (token.Index));
                }
                return new STJsonPathItem(STJsonPathExpressParser.GetSTJsonPathExpress(lst_range, false));
            }

            if (nLen > 3 && token.IsSymbol("?") && tokens[1].IsSymbol("(")) {
                var lst_range = STJsonPathTokenizer.GetRange(tokens, 1, "(", ")");
                if (lst_range.Count < 1) {
                    throw new STJsonPathParseException(token.Index + 1, "The bracket is empty. index: " + (token.Index + 1));
                }
                return new STJsonPathItem(STJsonPathExpressParser.GetSTJsonPathExpress(lst_range, true));
            }

            if (token.IsSymbol(":")) {
                return ME.GetSliceItem(tokens);
            }
            if (nLen > 1 && token.Type == STJsonPathTokenType.Long && tokens[1].IsSymbol(":")) {
                return ME.GetSliceItem(tokens);
            }

            List<int> lst_idx = new List<int>();
            List<string> lst_str = new List<string>();
            foreach (var v in tokens) {
                if (v.IsSymbol(",")) continue;
                switch (v.Type) {
                    case STJsonPathTokenType.String:
                    case STJsonPathTokenType.Property:
                    case STJsonPathTokenType.Keyword:
                        if (lst_idx.Count != 0) {
                            throw new STJsonPathParseException(token.Index, "All the value of array must be string or int. index: " + (token.Index));
                        }
                        lst_str.Add(v.Value);
                        continue;
                    case STJsonPathTokenType.Long:
                        if (lst_str.Count != 0) {
                            throw new STJsonPathParseException(token.Index, "All the value of array must be string or number. index: " + (token.Index));
                        }
                        lst_idx.Add(Convert.ToInt32(v.Value));
                        continue;
                    default:
                        throw new STJsonPathParseException(token.Index, "Unknows token [" + token.Value + "]");
                }
            }
            return new STJsonPathItem(lst_idx.ToArray(), lst_str.ToArray());
        }

        private static STJsonPathItem GetSliceItem(List<STJsonPathToken> tokens) {
            int nIndex = 0;
            int[] arr_index = new int[3] { 0, -1, 1 };
            for (int i = 0; i < arr_index.Length && nIndex < tokens.Count; i++) {
                if (tokens[nIndex].Type == STJsonPathTokenType.Long) {
                    arr_index[i] = Convert.ToInt32(tokens[nIndex++].Value);       // for index
                    if (nIndex >= tokens.Count) {
                        break;
                    }
                } else if (tokens[nIndex].Value != ":") {
                    throw new STJsonPathParseException(tokens[nIndex].Index, "Unknows token [" + tokens[nIndex].Value + "]");
                }
                nIndex++;
            }
            if (nIndex != tokens.Count) {
                throw new STJsonPathParseException(tokens[nIndex].Index, "Unknows token [" + tokens[nIndex].Value + "]");
            }
            if (arr_index[2] <= 0) {
                throw new STJsonPathParseException(tokens[nIndex - 1].Index, "The slice step must be more than zero. token [" + tokens[nIndex - 1].Value + "]");
            }
            return new STJsonPathItem(arr_index[0], arr_index[1], arr_index[2]);
        }
    }
}

