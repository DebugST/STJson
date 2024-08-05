using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ME = STLib.Json.STJsonPathExpression;

namespace STLib.Json
{
    internal class STJsonPathExpression
    {
        public string Text { get; private set; }
        public bool IsFilter { get; set; }
        public List<STJsonPathExpressionToken> Express { get; set; }

        public STJsonPathExpression(string strText, List<STJsonPathExpressionToken> lstExpTokens, bool isFilter)
        {
            this.Text = strText;
            this.Express = lstExpTokens;
            this.IsFilter = isFilter;
        }

        public STJsonPathExpressionToken Excute(STJson jsonRoot, STJson jsonCurrent)
        {
            return ME.Excute(this.Express, jsonRoot, jsonCurrent);
        }

        public STJsonPathExpressionToken ExcuteTest(STJson jsonRoot, STJson jsonCurrent, STJson jsonOut)
        {
            jsonOut.SetItem("type", "expression");
            jsonOut.SetItem("parsed", this.Text);
            var jsonPolish = jsonOut.SetKey("polish");
            foreach (var v in this.Express) {
                jsonPolish.Append(v.Text);
            }
            var token = ME.ExcuteTest(this.Express, jsonRoot, jsonCurrent, jsonOut);
            jsonOut.SetItem("return", new STJson()
                .SetItem("value_type", token.Type.ToString())
                .SetItem("bool_value", !(token.Type == STJsonPathExpressionTokenType.Undefined || token.Type == STJsonPathExpressionTokenType.Error || token.Value == null || token.Value.Equals(false)))
                );
            switch (token.Type) {
                case STJsonPathExpressionTokenType.Array:
                    jsonOut["return"].SetItem("value", STJson.CreateArray());
                    foreach (var v in token.Value as STJsonPathExpressionToken[]) {
                        jsonOut["return"]["value"].Append(v.Text);
                    }
                    break;
                default:
                    jsonOut["return"].SetItem("value", token.Text);
                    break;
            }
            return token;
        }

        // =====================================================================================================

        private static STJsonPathExpressionToken Excute(List<STJsonPathExpressionToken> tokens, STJson jsonRoot, STJson jsonCurrent)
        {
            STJsonPathExpressionToken token_l = STJsonPathExpressionToken.Undefined;
            STJsonPathExpressionToken token_r = STJsonPathExpressionToken.Undefined;
            Stack<STJsonPathExpressionToken> stack_result = new Stack<STJsonPathExpressionToken>();
            foreach (var obj in tokens) {
                switch (obj.Type) {
                    case STJsonPathExpressionTokenType.Operator:
                        token_r = stack_result.Pop();
                        token_l = stack_result.Pop();
                        token_l = ME.GetExpressToken(token_l, jsonRoot, jsonCurrent);
                        token_r = ME.GetExpressToken(token_r, jsonRoot, jsonCurrent);
                        if (token_l.Type == STJsonPathExpressionTokenType.Error) return token_l;
                        if (token_r.Type == STJsonPathExpressionTokenType.Error) return token_r;
                        //if (token_l.Type == ExpressTokenType.Undefined || token_r.Type == ExpressTokenType.Undefined) {
                        //    return ExpressionToken.Undefined;
                        //}
                        switch (obj.Value) {
                            case "+":
                            case "-":
                            case "*":
                            case "/":
                            case "%":
                                stack_result.Push(ME.ExcuteBasic(token_l, token_r, obj.Value.ToString()));
                                break;
                            case "&":
                            case "|":
                            case "<<":
                            case ">>":
                            case "^":
                            case "~":
                                stack_result.Push(ME.ExcuteBit(token_l, token_r, obj.Value.ToString()));
                                break;
                            case "!":
                            case ">":
                            case "<":
                            case ">=":
                            case "<=":
                            case "==":
                            case "!=":
                            case "&&":
                            case "||":
                            case "re":
                                stack_result.Push(ME.ExcuteLogic(token_l, token_r, obj.Value.ToString()));
                                break;
                            case "in":
                            case "nin":
                            case "anyof":
                                stack_result.Push(ME.ExcuteList(token_l, token_r, obj.Value.ToString()));
                                break;
                        }
                        break;
                    default:
                        stack_result.Push(obj);
                        break;

                }
            }
            if (stack_result.Count != 1) return STJsonPathExpressionToken.Undefined;
            var ret = stack_result.Pop();
            return ME.GetExpressToken(ret, jsonRoot, jsonCurrent);
        }

        private static STJsonPathExpressionToken GetExpressToken(STJsonPathExpressionToken token, STJson jsonRoot, STJson jsonCurrent)
        {
            switch (token.Type) {
                case STJsonPathExpressionTokenType.PathItem:
                    token = ME.GetExpressTokenFromPathItems(token, jsonRoot, jsonCurrent);
                    break;
                case STJsonPathExpressionTokenType.Func:
                    token = STJsonPathExpression.ExcuteFunc(token, jsonRoot, jsonCurrent);
                    break;
                case STJsonPathExpressionTokenType.ArrayExp:
                    token = ME.ExcuteArray(token, jsonRoot, jsonCurrent);
                    break;
            }
            return token;
        }

        private static STJsonPathExpressionToken GetExpressTokenFromPathItems(STJsonPathExpressionToken token, STJson jsonRoot, STJson jsonCurrent)
        {
            var json_result = STJson.CreateArray();
            SelectSetting setting = new SelectSetting()
            {
                Root = jsonRoot,
                Path = new Stack<object>(),
                Mode = STJsonPathSelectMode.ItemOnly
            };
            STJsonPath.GetSTJsons(token.PathItems, 0, jsonCurrent, setting, json_result);
            if (json_result.Count == 0) {
                return STJsonPathExpressionToken.Undefined;
            }
            return ME.GetExpressTokenFromSTJson(json_result[0]);
        }

        private static STJsonPathExpressionToken GetExpressTokenFromSTJson(STJson json)
        {
            STJsonPathExpressionToken[] arr_obj = null;
            if (json == null || json.IsNullValue) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Object, null);
            }
            switch (json.ValueType) {
                case STJsonValueType.Long:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, json.Value);
                case STJsonValueType.Double:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Double, json.Value);
                case STJsonValueType.String:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, json.Value);
                case STJsonValueType.Boolean:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Boolean, json.Value);
                case STJsonValueType.Datetime:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, ((DateTime)json.Value).ToString("O"));
                case STJsonValueType.Array:
                    arr_obj = new STJsonPathExpressionToken[json.Count];
                    for (int i = 0; i < json.Count; i++) {
                        arr_obj[i] = ME.GetExpressTokenFromSTJson(json[i]);
                    }
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Array, arr_obj);
            }
            return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Object, json);
        }

        private static STJsonPathExpressionToken ExcuteArray(STJsonPathExpressionToken token, STJson jsonRoot, STJson jsonCurrent)
        {
            STJsonPathExpression[] src = token.Value as STJsonPathExpression[];
            STJsonPathExpressionToken[] result = new STJsonPathExpressionToken[src.Length];
            for (int i = 0; i < src.Length; i++) {
                result[i] = src[i].Excute(jsonRoot, jsonCurrent);
            }
            return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Array, result);
        }

        private static STJsonPathExpressionToken ExcuteFunc(STJsonPathExpressionToken exp, STJson jsonRoot, STJson jsonCurrent)
        {
            var arr_exp_arg = new STJsonPathExpressionToken[exp.Args.Count];
            for (int i = 0; i < exp.Args.Count; i++) {
                arr_exp_arg[i] = exp.Args[i].Excute(jsonRoot, jsonCurrent);
            }
            string str_func_name = exp.Value.ToString();
            if (STJsonPath.CustomFunctions.ContainsKey(str_func_name)) {
                var arr_custom_arg = new STJsonPathExpressionFunctionArg[exp.Args.Count];
                for (int i = 0; i < arr_exp_arg.Length; i++) {
                    arr_custom_arg[i] = ME.ConvertExpressTokenToFuncArg(arr_exp_arg[i]);
                }
                var json = STJsonPath.CustomFunctions[str_func_name](arr_custom_arg);
                if (exp.PathItems != null && exp.PathItems.Count != 0) {
                    return ME.GetExpressTokenFromPathItems(exp, json, json);
                }
                return ME.GetExpressTokenFromSTJson(json);
            }
            if (STJsonPath.BuildInFunctions.ContainsKey(str_func_name)) {
                return STJsonPath.BuildInFunctions[str_func_name](arr_exp_arg);
            }
            return STJsonPathExpressionToken.CreateError("Can not match the function: " + str_func_name + "(" + STJsonPathBuildInFunctions.GetFunctionArgTypeList(arr_exp_arg) + "). ");
        }


        private static STJsonPathExpressionFunctionArg ConvertExpressTokenToFuncArg(STJsonPathExpressionToken token)
        {
            STJsonPathExpressionFunctionArg arg = new STJsonPathExpressionFunctionArg();
            if (token.Type == STJsonPathExpressionTokenType.Array) {
                var tokens = token.Value as STJsonPathExpressionToken[];
                STJsonPathExpressionFunctionArg[] arr_arg = new STJsonPathExpressionFunctionArg[tokens.Length];
                arg.Type = STJsonPathExpressionFunctionArgType.Array;
                for (int i = 0; i < arr_arg.Length; i++) {
                    arr_arg[i] = ME.ConvertExpressTokenToFuncArg(tokens[i]);
                }
                arg.Value = arr_arg;
                return arg;
            }
            if (token.Type <= STJsonPathExpressionTokenType.Object) {
                arg.Type = (STJsonPathExpressionFunctionArgType)token.Type;
                arg.Value = token.Value;
            }
            return arg;
        }

        private static STJsonPathExpressionToken ExcuteBasic(STJsonPathExpressionToken l, STJsonPathExpressionToken r, string strOP)
        {
            if (l.Type == STJsonPathExpressionTokenType.String || r.Type == STJsonPathExpressionTokenType.String) {
                if (strOP == "+") {
                    return STJsonPathExpressionToken.Create(
                        -1,
                        STJsonPathExpressionTokenType.String,
                        Convert.ToString(l.Value) + Convert.ToString(r.Value)
                        );
                }
                return STJsonPathExpressionToken.CreateError("The data types do not match and this operation cannot be completed. {" + l.Type + " " + strOP + " " + r.Type + "}");
            }
            if (l.IsNumber && r.IsNumber) {
                var token_ret = new STJsonPathExpressionToken() { Index = -1 };// ExpressionToken.Create(-1, ExpressTokenType.Double, 0);
                if (l.Type == STJsonPathExpressionTokenType.Double || r.Type == STJsonPathExpressionTokenType.Double) {
                    token_ret.Type = STJsonPathExpressionTokenType.Double;
                    double d_l = Convert.ToDouble(l.Value);
                    double d_r = Convert.ToDouble(r.Value);
                    switch (strOP) {
                        case "+": token_ret.Value = d_l + d_r; break;
                        case "-": token_ret.Value = d_l - d_r; break;
                        case "*": token_ret.Value = d_l * d_r; break;
                        case "/": token_ret.Value = d_l / d_r; break;
                        case "%":
                            return STJsonPathExpressionToken.CreateError("The data types do not match and this operation cannot be completed. {" + l.Type + " " + strOP + " " + r.Type + "}");
                    }
                } else {
                    token_ret.Type = STJsonPathExpressionTokenType.Long;
                    long l_l = Convert.ToInt64(l.Value);
                    long l_r = Convert.ToInt64(r.Value);
                    switch (strOP) {
                        case "+": token_ret.Value = l_l + l_r; break;
                        case "-": token_ret.Value = l_l - l_r; break;
                        case "*": token_ret.Value = l_l * l_r; break;
                        case "/": token_ret.Value = l_l / l_r; break;
                        case "%": token_ret.Value = l_l % l_r; break;
                    }
                }
                return token_ret;
            }
            return STJsonPathExpressionToken.CreateError("The data types do not match and this operation cannot be completed. {" + l.Type + " " + strOP + " " + r.Type + "}");
        }

        private static STJsonPathExpressionToken ExcuteBit(STJsonPathExpressionToken l, STJsonPathExpressionToken r, string strOP)
        {
            if (l.Type != STJsonPathExpressionTokenType.Long || r.Type != STJsonPathExpressionTokenType.Long) {
                return STJsonPathExpressionToken.CreateError("The data types do not match and this operation cannot be completed. {" + l.Type + " " + strOP + " " + r.Type + "}");
            }
            STJsonPathExpressionToken token_ret = new STJsonPathExpressionToken()
            {
                Type = STJsonPathExpressionTokenType.Long
            };
            long l_l = Convert.ToInt64(l.Value);
            long l_r = Convert.ToInt64(r.Value);
            switch (strOP) {
                case "&": token_ret.Value = l_l & l_r; break;
                case "|": token_ret.Value = l_l | l_r; break;
                case "<<": token_ret.Value = l_l << (int)l_r; break;
                case ">>": token_ret.Value = l_l >> (int)l_r; break;
                case "^": token_ret.Value = l_l ^ l_r; break;
                case "~": token_ret.Value = ~l_r; break;
            }
            return token_ret;
        }

        private static STJsonPathExpressionToken ExcuteLogic(STJsonPathExpressionToken l, STJsonPathExpressionToken r, string strOP)
        {
            STJsonPathExpressionToken token_ret = new STJsonPathExpressionToken()
            {
                Index = -1,
                Type = STJsonPathExpressionTokenType.Boolean
            };
            switch (strOP) {
                case "!": token_ret.Value = !Convert.ToBoolean(r.Value); return token_ret;
                case "&&": token_ret.Value = Convert.ToBoolean(l.Value) && Convert.ToBoolean(r.Value); return token_ret;
                case "||": token_ret.Value = Convert.ToBoolean(l.Value) || Convert.ToBoolean(r.Value); return token_ret;
                case "==":
                    if (l.IsNumber && r.IsNumber) {
                        token_ret.Value = Convert.ToDouble(l.Value) == Convert.ToDouble(r.Value);
                        return token_ret;
                    }
                    if (l.Type != r.Type) {
                        token_ret.Value = false;
                    } else {
                        token_ret.Value = Convert.ToString(l.Value) == Convert.ToString(r.Value);
                    }
                    return token_ret;
                case "!=":
                    if (l.IsNumber && r.IsNumber) {
                        token_ret.Value = Convert.ToDouble(l.Value) != Convert.ToDouble(r.Value);
                        return token_ret;
                    }
                    if (l.Type != r.Type) {
                        token_ret.Value = true;
                    } else {
                        token_ret.Value = Convert.ToString(l.Value) != Convert.ToString(r.Value);
                    }
                    return token_ret;
                case "re":
                    try {
                        token_ret.Value = Regex.IsMatch(Convert.ToString(l.Value), Convert.ToString(r.Value));
                    } catch { }
                    return token_ret;
            }
            if (!l.IsNumber || !r.IsNumber) {
                return STJsonPathExpressionToken.False;
            }
            if (l.Type == STJsonPathExpressionTokenType.Double || r.Type == STJsonPathExpressionTokenType.Double) {
                var d_l = Convert.ToDouble(l.Value);
                var d_r = Convert.ToDouble(r.Value);
                switch (strOP) {
                    case ">": token_ret.Value = d_l > d_r; break;
                    case "<": token_ret.Value = d_l < d_r; break;
                    case ">=": token_ret.Value = d_l >= d_r; break;
                    case "<=": token_ret.Value = d_l <= d_r; break;
                }
            } else {
                var l_l = Convert.ToInt64(l.Value);
                var l_r = Convert.ToInt64(r.Value);
                switch (strOP) {
                    case ">": token_ret.Value = l_l > l_r; break;
                    case "<": token_ret.Value = l_l < l_r; break;
                    case ">=": token_ret.Value = l_l >= l_r; break;
                    case "<=": token_ret.Value = l_l <= l_r; break;
                }
            }
            return token_ret;
        }

        private static STJsonPathExpressionToken ExcuteList(STJsonPathExpressionToken l, STJsonPathExpressionToken r, string strOP)
        {
            if (r.Type != STJsonPathExpressionTokenType.Array) {
                return STJsonPathExpressionToken.False;
            }
            STJsonPathExpressionToken[] arr_l = null;
            STJsonPathExpressionToken[] arr_r = r.Value as STJsonPathExpressionToken[];
            if (l.Type == STJsonPathExpressionTokenType.Array) {
                arr_l = l.Value as STJsonPathExpressionToken[];
            } else {
                arr_l = new STJsonPathExpressionToken[] { l };
            }
            int nCounter = 0;
            switch (strOP) {
                case "in":
                    foreach (var obj_l in arr_l) {
                        foreach (var obj_r in arr_r) {
                            if (object.Equals(obj_l.Value, obj_r.Value)) {
                                nCounter++;
                                break;
                            }
                        }
                    }
                    return nCounter == arr_l.Length ? STJsonPathExpressionToken.True : STJsonPathExpressionToken.False;
                case "nin":
                    foreach (var obj_l in arr_l) {
                        foreach (var obj_r in arr_r) {
                            if (object.Equals(obj_l.Value, obj_r.Value)) {
                                return STJsonPathExpressionToken.False;
                            }
                        }
                    }
                    return STJsonPathExpressionToken.True;
                case "anyof":
                    foreach (var obj_l in arr_l) {
                        foreach (var obj_r in arr_r) {
                            if (object.Equals(obj_l.Value, obj_r.Value)) {
                                return STJsonPathExpressionToken.True;
                            }
                        }
                    }
                    return STJsonPathExpressionToken.False;
            }
            return STJsonPathExpressionToken.False;
        }

        // =====================================================================================================

        private static STJsonPathExpressionToken ExcuteTest(List<STJsonPathExpressionToken> tokens, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut)
        {
            STJsonPathExpressionToken token_l = STJsonPathExpressionToken.Undefined;
            STJsonPathExpressionToken token_r = STJsonPathExpressionToken.Undefined;
            STJsonPathExpressionToken token_result = STJsonPathExpressionToken.Undefined;
            Stack<STJsonPathExpressionToken> stack_result = new Stack<STJsonPathExpressionToken>();
            STJson jsonStep = null;
            STJson jsonSteps = STJson.CreateArray();
            jsonOut.SetItem("steps", jsonSteps);
            foreach (var obj in tokens) {
                switch (obj.Type) {
                    case STJsonPathExpressionTokenType.Operator:
                        jsonStep = new STJson();
                        jsonStep.SetItem("type", "excute");
                        jsonStep.SetItem("operator", obj.Value);
                        jsonSteps.Append(jsonStep);
                        token_r = stack_result.Pop();
                        token_l = stack_result.Pop();
                        token_l = ME.GetExpressTokenTest(token_l, jsonRoot, jsonCurrent, jsonStep.SetKey("get_left_token"));
                        token_r = ME.GetExpressTokenTest(token_r, jsonRoot, jsonCurrent, jsonStep.SetKey("get_right_token"));
                        if (token_l.Type == STJsonPathExpressionTokenType.Error) return token_l;
                        if (token_r.Type == STJsonPathExpressionTokenType.Error) return token_r;
                        //if (token_l.Type == STJsonPathExpressionTokenType.Undefined || token_r.Type == STJsonPathExpressionTokenType.Undefined) {
                        //    return STJsonPathExpressionToken.Undefined;
                        //}
                        switch (obj.Value) {
                            case "+":
                            case "-":
                            case "*":
                            case "/":
                            case "%":
                                token_result = ME.ExcuteBasic(token_l, token_r, obj.Value.ToString());
                                break;
                            case "&":
                            case "|":
                            case "<<":
                            case ">>":
                            case "^":
                            case "~":
                                token_result = ME.ExcuteBit(token_l, token_r, obj.Value.ToString());
                                break;
                            case "!":
                            case ">":
                            case "<":
                            case ">=":
                            case "<=":
                            case "==":
                            case "!=":
                            case "&&":
                            case "||":
                            case "re":
                                token_result = ME.ExcuteLogic(token_l, token_r, obj.Value.ToString());
                                break;
                            case "in":
                            case "nin":
                            case "anyof":
                                token_result = ME.ExcuteList(token_l, token_r, obj.Value.ToString());
                                break;
                        }
                        jsonStep.SetItem("result", new STJson().SetItem("value_type", token_result.Type.ToString()).SetItem("text", token_result.Text));
                        stack_result.Push(token_result);
                        break;
                    default:
                        stack_result.Push(obj);
                        break;

                }
            }
            if (stack_result.Count != 1) {
                jsonOut.SetItem("error", "Empty expression.");
                return STJsonPathExpressionToken.Undefined;
            }
            var ret = stack_result.Pop();
            return ME.GetExpressTokenTest(ret, jsonRoot, jsonCurrent, jsonOut.SetKey("check_result"));
        }

        private static STJsonPathExpressionToken GetExpressTokenTest(STJsonPathExpressionToken token, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut)
        {
            switch (token.Type) {
                case STJsonPathExpressionTokenType.PathItem:
                    jsonOut.SetItem("type", "selector");
                    token = ME.GetExpressTokenFromPathItemsTest(token, jsonRoot, jsonCurrent, jsonOut);
                    break;
                case STJsonPathExpressionTokenType.Func:
                    jsonOut.SetItem("type", "function");
                    token = STJsonPathExpression.ExcuteFuncTest(token, jsonRoot, jsonCurrent, jsonOut);
                    break;
                case STJsonPathExpressionTokenType.ArrayExp:
                    jsonOut.SetItem("type", "array_expression");
                    token = ME.ExcuteArrayTest(token, jsonRoot, jsonCurrent, jsonOut);
                    break;
                case STJsonPathExpressionTokenType.Error:
                    jsonOut.SetItem("type", "error");
                    break;
                default:
                    jsonOut.SetItem("type", "value");
                    break;
            }
            jsonOut.SetItem("parsed", token.Text);
            jsonOut.SetItem("result", new STJson().SetItem("value_type", token.Type.ToString()).SetItem("text", token.Text));
            return token;
        }

        private static STJsonPathExpressionToken GetExpressTokenFromPathItemsTest(STJsonPathExpressionToken token, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut)
        {
            var json_result = STJson.CreateArray();
            SelectSetting setting = new SelectSetting()
            {
                Root = jsonRoot,
                Path = new Stack<object>(),
                Mode = STJsonPathSelectMode.ItemOnly
            };
            STJsonPath.GetSTJsons(token.PathItems, 0, jsonCurrent, setting, json_result);
            jsonOut.SetItem("root_json", jsonRoot == null ? null : jsonRoot.ToString());
            jsonOut.SetItem("current_json", jsonCurrent == null ? null : jsonCurrent.ToString());
            jsonOut.SetItem("selected_json", json_result.ToString());
            if (json_result.Count == 0) {
                return STJsonPathExpressionToken.Undefined;
            }
            var token_ret = ME.GetExpressTokenFromSTJson(json_result[0]);
            return token_ret;
        }

        private static STJsonPathExpressionToken ExcuteArrayTest(STJsonPathExpressionToken token, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut)
        {
            var jsonItems = STJson.CreateArray();
            jsonOut.SetItem("get_items", jsonItems);
            STJsonPathExpression[] src = token.Value as STJsonPathExpression[];
            STJsonPathExpressionToken[] result = new STJsonPathExpressionToken[src.Length];
            for (int i = 0; i < src.Length; i++) {
                var jsonItem = new STJson();
                result[i] = src[i].ExcuteTest(jsonRoot, jsonCurrent, jsonItem);
                jsonItems.Append(jsonItem);
            }
            return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Array, result);
        }

        private static STJsonPathExpressionToken ExcuteFuncTest(STJsonPathExpressionToken exp, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut)
        {
            var token_return = STJsonPathExpressionToken.Undefined;
            var jsonArgs = STJson.CreateArray();
            List<object> lst_args = new List<object>();
            jsonOut.SetItem("get_args", jsonArgs);
            var arr_exp_arg = new STJsonPathExpressionToken[exp.Args.Count];
            for (int i = 0; i < exp.Args.Count; i++) {
                var jsonArg = new STJson();
                arr_exp_arg[i] = exp.Args[i].ExcuteTest(jsonRoot, jsonCurrent, jsonArg);
                jsonArgs.Append(jsonArg);
                lst_args.Add(arr_exp_arg[i].Text);
            }
            jsonOut.SetItem("args_text", lst_args);
            string str_func_name = exp.Value.ToString();
            if (STJsonPath.CustomFunctions.ContainsKey(str_func_name)) {
                jsonOut.SetItem("type", "custom_function");
                var arr_custom_arg = new STJsonPathExpressionFunctionArg[exp.Args.Count];
                for (int i = 0; i < arr_exp_arg.Length; i++) {
                    arr_custom_arg[i] = ME.ConvertExpressTokenToFuncArg(arr_exp_arg[i]);
                }
                var json = STJsonPath.CustomFunctions[str_func_name](arr_custom_arg);
                jsonOut.SetItem("result_json", json.ToString());
                if (exp.PathItems != null && exp.PathItems.Count != 0) {
                    token_return = ME.GetExpressTokenFromPathItemsTest(exp, json, json, jsonOut);
                } else {
                    token_return = ME.GetExpressTokenFromSTJson(json);
                }
                return token_return;
            }
            if (STJsonPath.BuildInFunctions.ContainsKey(str_func_name)) {
                jsonOut.SetItem("type", "buildin_function");
                return STJsonPath.BuildInFunctions[str_func_name](arr_exp_arg);
            }
            jsonOut.SetItem("error", "Can not match the function: " + str_func_name + "(" + STJsonPathBuildInFunctions.GetFunctionArgTypeList(arr_exp_arg) + ").");
            return STJsonPathExpressionToken.CreateError("Can not match the function: " + str_func_name + "(" + STJsonPathBuildInFunctions.GetFunctionArgTypeList(arr_exp_arg) + ").");
        }
    }
}
