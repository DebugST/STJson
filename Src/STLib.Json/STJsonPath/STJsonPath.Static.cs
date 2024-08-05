using System;
using System.Collections.Generic;
using System.Linq;

using ME = STLib.Json.STJsonPath;

namespace STLib.Json
{
    public partial class STJsonPath
    {
        public static Dictionary<string, STJsonPathCustomFunctionHander> CustomFunctions { get; private set; }
        internal static Dictionary<string, STJsonPathBuildInFuncHander> BuildInFunctions { get; private set; }

        static STJsonPath()
        {
            ME.BuildInFunctions = new Dictionary<string, STJsonPathBuildInFuncHander>();
            ME.CustomFunctions = new Dictionary<string, STJsonPathCustomFunctionHander>();
            STJsonPathBuildInFunctions.Init();
        }

        private static STJson GetParsedTokens(List<STJsonPathItem> pathItems)
        {
            STJson json = STJson.CreateArray();
            foreach (var v in pathItems) {
                ME.GetParsedTokens(json, v);
            }
            return json;
        }

        private static void GetParsedTokens(STJson json, STJsonPathItem item)
        {
            STJson jsonTemp = null;
            switch (item.Type) {
                case STJsonPathItem.ItemType.Root:
                    json.Append(STJson.FromObject(new { type = "selector_item", item_type = item.Type.ToString(), value = "$" }));
                    break;
                case STJsonPathItem.ItemType.Current:
                    json.Append(STJson.FromObject(new { type = "selector_item", item_type = item.Type.ToString(), value = "@" }));
                    break;
                case STJsonPathItem.ItemType.Depth:
                    json.Append(STJson.FromObject(new { type = "selector_item", item_type = item.Type.ToString(), value = ".." }));
                    break;
                case STJsonPathItem.ItemType.Any:
                    json.Append(STJson.FromObject(new { type = "selector_item", item_type = item.Type.ToString(), value = "*" }));
                    break;
                case STJsonPathItem.ItemType.Slice:
                    json.Append(STJson.FromObject(new { type = "selector_item", item_type = item.Type.ToString(), value = "[" + item.Start + ":" + item.End + ":" + item.Step + "]" }));
                    break;
                case STJsonPathItem.ItemType.Express:
                    json.Append(ME.GetParseExpress(item.Express));
                    break;
                case STJsonPathItem.ItemType.List:
                    jsonTemp = STJson.CreateArray();
                    foreach (var v in item.Indices) {
                        jsonTemp.Append(v);
                    }
                    foreach (var v in item.Keys) {
                        jsonTemp.Append(v);
                    }
                    json.Append(STJson.FromObject(new { type = "selector_item", item_type = item.Type.ToString(), value = jsonTemp }));
                    break;
            }
        }

        private static STJson GetParseExpress(STJsonPathExpression express)
        {
            var json_items = STJson.CreateArray();
            STJson json_array = null;
            var json = new STJson();
            json.SetItem("type", "expression");
            json.SetItem("parsed", express.Text);
            json.SetItem("items", json_items);
            foreach (var v in express.Express) {
                switch (v.Type) {
                    case STJsonPathExpressionTokenType.Operator:
                        json_items.Append(STJson.FromObject(new { type = "expression_item", item_type = "operator", value = v.Value }));
                        break;
                    case STJsonPathExpressionTokenType.String:
                        json_items.Append(STJson.FromObject(new { type = "expression_item", item_type = "string", value = v.Value }));
                        break;
                    case STJsonPathExpressionTokenType.Long:
                        json_items.Append(STJson.FromObject(new { type = "expression_item", item_type = "long", value = v.Value }));
                        break;
                    case STJsonPathExpressionTokenType.Double:
                        json_items.Append(STJson.FromObject(new { type = "expression_item", item_type = "double", value = v.Value }));
                        break;
                    case STJsonPathExpressionTokenType.Boolean:
                        json_items.Append(STJson.FromObject(new { type = "expression_item", item_type = "boolean", value = v.Value }));
                        break;
                    case STJsonPathExpressionTokenType.Array:
                        json_array = STJson.CreateArray();
                        json_items.Append(STJson.FromObject(new { type = "expression_item", item_type = "array", value = json_array }));
                        foreach (STJsonPathExpression exp in v.Value as STJsonPathExpression[]) {
                            json_array.Append(ME.GetParseExpress(exp));
                        }
                        break;
                    case STJsonPathExpressionTokenType.PathItem:
                        json_items.Append(STJson.FromObject(new
                        {
                            type = "expression_item",
                            item_type = "selector",
                            items = v.PathItems == null || v.PathItems.Count == 0 ? null : ME.GetParsedTokens(v.PathItems)
                        }));
                        break;
                    case STJsonPathExpressionTokenType.Func:
                        json_items.Append(ME.GetParseExpressFunction(v));
                        break;
                }
            }
            return json;
        }

        private static STJson GetParseExpressFunction(STJsonPathExpressionToken token)
        {
            var strText = string.Empty;
            STJson json_arg = null;
            if (token.Args != null && token.Args.Count > 0) {
                json_arg = new STJson().SetItem("parsed", "").SetItem("items", STJson.CreateArray());
                foreach (var v in token.Args) {
                    strText += v.Text + ", ";
                    json_arg["items"].Append(ME.GetParseExpress(v));
                }
                strText = strText.Trim().Trim(',');
                json_arg.SetItem("parsed", "(" + strText + ")");
            }

            strText = String.Empty;
            STJson json_selector = null;
            if (token.PathItems != null && token.PathItems.Count > 0) {
                json_selector = new STJson().SetItem("parsed", "").SetItem("items", STJson.CreateArray());
                foreach (var v in token.PathItems) {
                    strText += v.Text;
                    json_selector["items"].SetValue(ME.GetParsedTokens(token.PathItems));
                }
                json_selector.SetItem("parsed", strText);
            }
            return new STJson()
                .SetItem("type", "function")
                .SetItem("parsed", token.Text)
                .SetItem("name", token.Value)
                .SetItem("args", json_arg)
                .SetItem("selector", json_selector);
        }

        internal static void GetSTJsons(List<STJsonPathItem> items, int nIndex, STJson jsonCurrent, SelectSetting setting, STJson jsonOut)
        {
            if (nIndex == items.Count) {
                ME.OnSelectedItem(jsonCurrent, setting, jsonOut);
                return;
            }
            int nLoopIndex = 0;
            STJsonPathExpressionToken exp_ret = STJsonPathExpressionToken.Undefined;
            var item = items[nIndex];
            var nIndexSliceL = item.Start;
            var nIndexSliceR = item.End;
            switch (item.Type) {
                case STJsonPathItem.ItemType.Root:
                case STJsonPathItem.ItemType.Current:
                    setting.Path.Push(item.Type == STJsonPathItem.ItemType.Root ? "$" : "@");
                    ME.GetSTJsons(
                        items,
                        nIndex + 1,
                        item.Type == STJsonPathItem.ItemType.Current ? jsonCurrent : setting.Root,
                        setting,
                        jsonOut);
                    break;
                case STJsonPathItem.ItemType.Any:
                    if (jsonCurrent == null) return;
                    foreach (var v in jsonCurrent) {
                        setting.Path.Push(jsonCurrent.ValueType == STJsonValueType.Array ? (object)nLoopIndex : (object)v.Key);
                        ME.GetSTJsons(items, nIndex + 1, v, setting, jsonOut);
                        setting.Path.Pop();
                        nLoopIndex++;
                    }
                    break;
                case STJsonPathItem.ItemType.Depth:
                    ME.GetSTJsons(items, nIndex + 1, jsonCurrent, setting, jsonOut);
                    if (jsonCurrent == null) return;
                    foreach (var v in jsonCurrent) {
                        setting.Path.Push(jsonCurrent.ValueType == STJsonValueType.Array ? (object)nLoopIndex : (object)v.Key);
                        ME.GetSTJsons(items, nIndex, v, setting, jsonOut);
                        setting.Path.Pop();
                        nLoopIndex++;
                    }
                    break;
                case STJsonPathItem.ItemType.List:
                    if (jsonCurrent == null) return;
                    if (jsonCurrent.ValueType == STJsonValueType.Object) {
                        foreach (var v in item.Keys) {
                            if (jsonCurrent[v] == null) {
                                continue;
                            }
                            setting.Path.Push(v);
                            ME.GetSTJsons(items, nIndex + 1, jsonCurrent[v], setting, jsonOut);
                            setting.Path.Pop();
                        }
                    }
                    if (jsonCurrent.ValueType == STJsonValueType.Array) {
                        foreach (var v in item.Indices) {
                            nIndexSliceL = v;
                            if (nIndexSliceL < 0) nIndexSliceL = jsonCurrent.Count + nIndexSliceL;
                            if (nIndexSliceL < 0) continue;
                            if (nIndexSliceL >= jsonCurrent.Count) continue;
                            setting.Path.Push(nIndexSliceL);
                            ME.GetSTJsons(items, nIndex + 1, jsonCurrent[nIndexSliceL], setting, jsonOut);
                            setting.Path.Pop();
                        }
                    }
                    break;
                case STJsonPathItem.ItemType.Slice:
                    if (jsonCurrent == null) return;
                    if (jsonCurrent.ValueType != STJsonValueType.Array || jsonCurrent.Count == 0) {
                        return;
                    }
                    if (nIndexSliceL < 0) nIndexSliceL = jsonCurrent.Count + nIndexSliceL;
                    if (nIndexSliceR < 0) nIndexSliceR = jsonCurrent.Count + nIndexSliceR;
                    if (nIndexSliceL < 0) nIndexSliceL = 0;
                    else if (nIndexSliceL >= jsonCurrent.Count) nIndexSliceL = jsonCurrent.Count - 1;
                    if (nIndexSliceR < 0) nIndexSliceR = 0;
                    else if (nIndexSliceR >= jsonCurrent.Count) nIndexSliceR = jsonCurrent.Count - 1;
                    if (nIndexSliceL > nIndexSliceR) {
                        for (int i = nIndexSliceL; i >= nIndexSliceR; i -= item.Step) {
                            setting.Path.Push(i);
                            ME.GetSTJsons(items, nIndex + 1, jsonCurrent[i], setting, jsonOut);
                            setting.Path.Pop();
                        }
                    } else {
                        for (int i = nIndexSliceL; i <= nIndexSliceR; i += item.Step) {
                            setting.Path.Push(i);
                            ME.GetSTJsons(items, nIndex + 1, jsonCurrent[i], setting, jsonOut);
                            setting.Path.Pop();
                        }
                    }
                    break;
                case STJsonPathItem.ItemType.Express:
                    exp_ret = item.Express.Excute(setting.Root, jsonCurrent);
                    if (exp_ret.Type == STJsonPathExpressionTokenType.Error || exp_ret.Type == STJsonPathExpressionTokenType.Undefined || exp_ret.Value == null) {
                        return;
                    }
                    if (item.Express.IsFilter) {
                        if (exp_ret.Value.Equals(false)) {
                            return;
                        }
                        ME.GetSTJsons(items, nIndex + 1, jsonCurrent, setting, jsonOut);
                    } else {
                        if (jsonCurrent == null) return;
                        if (exp_ret.Type == STJsonPathExpressionTokenType.Long) {
                            if (jsonCurrent.ValueType != STJsonValueType.Array) {
                                return;
                            }
                            nIndexSliceL = Convert.ToInt32(exp_ret.Value);
                            if (nIndexSliceL < 0 || nIndexSliceL >= jsonCurrent.Count) {
                                return;
                            }
                            setting.Path.Push(nIndexSliceL);
                            ME.GetSTJsons(items, nIndex + 1, jsonCurrent[nIndexSliceL], setting, jsonOut);
                            setting.Path.Pop();
                        }
                        if (exp_ret.Type == STJsonPathExpressionTokenType.String) {
                            jsonCurrent = jsonCurrent[exp_ret.Value.ToString()];
                            if (jsonCurrent == null) {
                                return;
                            }
                            setting.Path.Push(exp_ret.Value.ToString());
                            ME.GetSTJsons(items, nIndex + 1, jsonCurrent, setting, jsonOut);
                            setting.Path.Pop();
                        }
                        if (exp_ret.Type == STJsonPathExpressionTokenType.Array) {
                            foreach (var v in exp_ret.Value as STJsonPathExpressionToken[]) {
                                switch (jsonCurrent.ValueType) {
                                    case STJsonValueType.Object:
                                        if (v.Type != STJsonPathExpressionTokenType.String || v.Value == null) {
                                            continue;
                                        }
                                        if (jsonCurrent[v.Value.ToString()] == null) {
                                            continue;
                                        }
                                        setting.Path.Push(v.Value.ToString());
                                        ME.GetSTJsons(items, nIndex + 1, jsonCurrent[v.Value.ToString()], setting, jsonOut);
                                        setting.Path.Pop();
                                        continue;
                                    case STJsonValueType.Array:
                                        if (v.Type != STJsonPathExpressionTokenType.Long || v.Value == null) {
                                            continue;
                                        }
                                        nIndexSliceL = Convert.ToInt32(v.Value);
                                        if (nIndexSliceL < 0 || nIndexSliceL >= jsonCurrent.Count) {
                                            continue;
                                        }
                                        setting.Path.Push(nIndexSliceL);
                                        ME.GetSTJsons(items, nIndex + 1, jsonCurrent[nIndexSliceL], setting, jsonOut);
                                        setting.Path.Pop();
                                        continue;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        internal static void OnSelectedItem(STJson item, SelectSetting setting, STJson jsonOut)
        {
            STJson jsonPath = STJson.CreateArray();
            Dictionary<int, Dictionary<int, int>> dic_index = new Dictionary<int, Dictionary<int, int>>();
            switch (setting.Mode) {
                case STJsonPathSelectMode.ItemWithPath:
                case STJsonPathSelectMode.KeepStructure:
                    foreach (var v in setting.Path.Reverse()) {
                        jsonPath.Append(v);
                    }
                    break;
            }
            if (setting.Callback != null) {
                if (jsonPath.Count == 0) {
                    foreach (var v in setting.Path.Reverse()) {
                        jsonPath.Append(v);
                    }
                }
                var arg = new STJsonPathCallbackArgs()
                {
                    Selected = true,
                    Path = jsonPath,
                    Json = item
                };
                setting.Callback(arg);
                if (!arg.Selected) {
                    return;
                }
                item = arg.Json;
            }
            //if (setting.CallbackReturn != null || setting.CallbackVoid != null) {
            //    if (jsonPath.Count == 0) {
            //        foreach (var v in setting.Path.Reverse()) {
            //            jsonPath.Append(v);
            //        }
            //    }
            //}
            //if (setting.CallbackVoid != null) {
            //    setting.CallbackVoid(jsonPath, item);
            //    return;
            //}
            //if (setting.CallbackReturn != null) {
            //    var cbr = setting.CallbackReturn(jsonPath, item);
            //    if (!cbr.Selected) {
            //        return;
            //    }
            //    item = cbr.Json;
            //}
            switch (setting.Mode) {
                case STJsonPathSelectMode.ItemOnly:
                    jsonOut.Append(item);
                    break;
                case STJsonPathSelectMode.KeepStructure:
                case STJsonPathSelectMode.ItemWithPath:
                    jsonOut.Append(new STJson()
                        .SetItem("path", jsonPath)
                        .SetItem("item", item)
                        );
                    break;
            }
        }

        internal static STJson CreatePathJson(STJson value, List<STJsonPathItem> items)
        {
            STJson jsonOut = new STJson();
            Stack<object> stackPath = new Stack<object>();
            //ME.CreatePathJson((p, j) => {
            //    return new STJsonPathCallBackResult() {
            //        Selected = true,
            //        Json = value
            //    };
            //}, stackPath, items, 0, jsonOut);
            ME.CreatePathJson((arg) =>
            {
                arg.Selected = true;
                arg.Json = value;
            }, stackPath, items, 0, jsonOut);
            return jsonOut;
        }

        internal static STJson CreatePathJson(STJsonPathCallBack callBack, List<STJsonPathItem> items)
        {
            STJson jsonOut = new STJson();
            Stack<object> stackPath = new Stack<object>();
            ME.CreatePathJson(callBack, stackPath, items, 0, jsonOut);
            return jsonOut;
        }

        internal static void CreatePathJson(STJsonPathCallBack callBack, Stack<object> stackPath, List<STJsonPathItem> items, int nIndex, STJson jsonOut)
        {
            if (nIndex >= items.Count) {
                STJson jsonPath = STJson.CreateArray();
                foreach (var v in stackPath.Reverse()) {
                    jsonPath.Append(v);
                }
                var arg = new STJsonPathCallbackArgs()
                {
                    Selected = true,
                    Path = jsonPath
                };
                callBack(arg);
                //var ret = callBack(jsonPath, null);
                if (!arg.Selected) {
                    return;
                }
                jsonOut.Append(new STJson()
                       .SetItem("path", jsonPath)
                       .SetItem("item", arg.Json)
                       );
                return;
            }
            var item = items[nIndex];
            int nIndexSliceL = item.Start;
            int nIndexSliceR = item.End;
            switch (item.Type) {
                case STJsonPathItem.ItemType.List:
                    foreach (var v in item.Indices) {
                        stackPath.Push(v);
                        ME.CreatePathJson(callBack, stackPath, items, nIndex + 1, jsonOut);
                        stackPath.Pop();
                    }
                    foreach (var v in item.Keys) {
                        stackPath.Push(v);
                        ME.CreatePathJson(callBack, stackPath, items, nIndex + 1, jsonOut);
                        stackPath.Pop();
                    }
                    break;
                case STJsonPathItem.ItemType.Slice:
                    if (nIndexSliceL < 0) nIndexSliceL = -nIndexSliceL;
                    if (nIndexSliceR < 0) nIndexSliceR = -nIndexSliceR;
                    if (nIndexSliceL > nIndexSliceR) {
                        for (int i = nIndexSliceL; i >= nIndexSliceR; i -= item.Step) {
                            stackPath.Push(i);
                            ME.CreatePathJson(callBack, stackPath, items, nIndex + 1, jsonOut);
                            stackPath.Pop();
                        }
                    } else {
                        for (int i = nIndexSliceL; i <= nIndexSliceR; i += item.Step) {
                            stackPath.Push(i);
                            ME.CreatePathJson(callBack, stackPath, items, nIndex + 1, jsonOut);
                            stackPath.Pop();
                        }
                    }
                    break;
                default:
                    throw new STJsonPathException("Can not support {" + item.Text + "}, when call CreatePathJson()");
            }
        }

        public static STJson RestorePathJson(STJson jsonItemWithPath)
        {
            STJson jsonResult = new STJson();
            ME.RestorePathJson(jsonResult, jsonItemWithPath);
            return jsonResult;
        }

        public static void RestorePathJson(STJson jsonResult, STJson jsonItemWithPath)
        {
            Dictionary<string, Dictionary<int, int>> dicIndices = new Dictionary<string, Dictionary<int, int>>();
            foreach (var item in jsonItemWithPath) {
                var path = item["path"];
                var jsonNext = jsonResult;
                var nIndex = 0;
                Dictionary<int, int> dicTemp = null;
                string strKey = string.Empty;
                for (int i = 0; i < path.Count; i++) {
                    switch (path[i].ValueType) {
                        case STJsonValueType.Long:
                            if (!dicIndices.ContainsKey(strKey)) {
                                dicTemp = new Dictionary<int, int>();
                                dicIndices.Add(strKey, dicTemp);
                            } else {
                                dicTemp = dicIndices[strKey];
                            }
                            nIndex = path[i].GetValue<int>();
                            if (dicTemp.ContainsKey(nIndex)) {
                                jsonNext = jsonNext[dicTemp[nIndex]];
                            } else {
                                jsonNext.Append(new STJson());
                                jsonNext = jsonNext[dicTemp.Count];
                                dicTemp.Add(nIndex, dicTemp.Count());
                            }
                            break;
                        case STJsonValueType.String:
                            jsonNext = jsonNext.SetKey(path[i].GetValue());
                            break;
                    }
                    strKey += path[i].GetValue() + ",";
                }
                jsonNext.SetValue(item["item"]);
            }
        }

        public static STJson TestExpression(STJson jsonRoot, STJson jsonCurrent, string strExpression)
        {
            STJson jsonResult = new STJson();
            var tokens = STJsonPathTokenizer.GetTokens(strExpression);
            var expression = STJsonPathExpressParser.GetSTJsonPathExpress(tokens, false);
            if (jsonRoot == null) {
                jsonRoot = jsonCurrent;
            }
            if (jsonCurrent == null) {
                jsonCurrent = jsonRoot;
            }
            expression.ExcuteTest(jsonRoot, jsonCurrent, jsonResult);
            return jsonResult;
        }

        public static STJson GetBuildInFunctionList()
        {
            return STJsonPathBuildInFunctions.FuncList.Clone();
        }
    }
}

