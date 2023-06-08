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
        public List<ExpressionToken> Express { get; set; }

        public STJsonPathExpression(string strText, List<ExpressionToken> lstExpTokens, bool isFilter) {
            this.Text = strText;
            this.Express = lstExpTokens;
            this.IsFilter = isFilter;
        }

        public ExpressionToken Excute(STJson jsonRoot, STJson jsonCurrent) {
            return ME.Excute(this.Express, jsonRoot, jsonCurrent);
        }

        public ExpressionToken ExcuteTest(STJson jsonRoot, STJson jsonCurrent, STJson jsonOut) {
            jsonOut.SetItem("type", "expression");
            jsonOut.SetItem("parsed", this.Text);
            var jsonPolish = jsonOut.SetKey("polish");
            foreach (var v in this.Express) {
                jsonPolish.Append(v.Text);
            }
            var token = ME.ExcuteTest(this.Express, jsonRoot, jsonCurrent, jsonOut);
            jsonOut.SetItem("return", new STJson()
                .SetItem("bool", !(token.Type == ExpressTokenType.Undefined || token.Value == null || token.Value.Equals(false)))
                .SetItem("value_type", token.Type.ToString())
                );
            switch (token.Type) {
                case ExpressTokenType.Array:
                    jsonOut["return"].SetItem("items", STJson.CreateArray());
                    foreach (var v in token.Value as ExpressionToken[]) {
                        jsonOut["return"]["items"].Append(v.Text);
                    }
                    break;
                default:
                    jsonOut["return"].SetItem("text", token.Text);
                    break;
            }
            return token;
        }

        // =====================================================================================================

        private static ExpressionToken Excute(List<ExpressionToken> tokens, STJson jsonRoot, STJson jsonCurrent) {
            ExpressionToken token_l = ExpressionToken.Undefined;
            ExpressionToken token_r = ExpressionToken.Undefined;
            Stack<ExpressionToken> stack_result = new Stack<ExpressionToken>();
            foreach (var obj in tokens) {
                switch (obj.Type) {
                    case ExpressTokenType.Operator:
                        token_r = stack_result.Pop();
                        token_l = stack_result.Pop();
                        token_l = ME.GetExpressToken(token_l, jsonRoot, jsonCurrent);
                        token_r = ME.GetExpressToken(token_r, jsonRoot, jsonCurrent);
                        if (token_l.Type == ExpressTokenType.Undefined || token_r.Type == ExpressTokenType.Undefined) {
                            return ExpressionToken.Undefined;
                        }
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
            if (stack_result.Count != 1) return ExpressionToken.Undefined;
            var ret = stack_result.Pop();
            return ME.GetExpressToken(ret, jsonRoot, jsonCurrent);
        }

        private static ExpressionToken GetExpressToken(ExpressionToken token, STJson jsonRoot, STJson jsonCurrent) {
            switch (token.Type) {
                case ExpressTokenType.PathItem:
                    token = ME.GetExpressTokenFromPathItems(token, jsonRoot, jsonCurrent);
                    break;
                case ExpressTokenType.Func:
                    token = STJsonPathExpression.ExcuteFunc(token, jsonRoot, jsonCurrent);
                    break;
                case ExpressTokenType.ArrayExp:
                    token = ME.ExcuteArray(token, jsonRoot, jsonCurrent);
                    break;
            }
            return token;
        }

        private static ExpressionToken GetExpressTokenFromPathItems(ExpressionToken token, STJson jsonRoot, STJson jsonCurrent) {
            var json_result = STJson.CreateArray();
            SelectSetting setting = new SelectSetting() {
                Root = jsonRoot,
                Mode = STJsonPathSelectMode.ItemOnly
            };
            STJsonPath.GetSTJsons(token.PathItems, 0, jsonCurrent, setting, json_result);
            if (json_result.Count == 0) {
                return ExpressionToken.Undefined;
            }
            return ME.GetExpressTokenFromSTJson(json_result[0]);
        }

        private static ExpressionToken GetExpressTokenFromSTJson(STJson json) {
            ExpressionToken[] arr_obj = null;
            if (json == null || json.IsNullObject) {
                return ExpressionToken.Create(-1, ExpressTokenType.Object, null);
            }
            switch (json.ValueType) {
                case STJsonValueType.Long:
                    return ExpressionToken.Create(-1, ExpressTokenType.Long, json.Value);
                case STJsonValueType.Double:
                    return ExpressionToken.Create(-1, ExpressTokenType.Double, json.Value);
                case STJsonValueType.String:
                    return ExpressionToken.Create(-1, ExpressTokenType.String, json.Value);
                case STJsonValueType.Boolean:
                    return ExpressionToken.Create(-1, ExpressTokenType.Boolean, json.Value);
                case STJsonValueType.Datetime:
                    return ExpressionToken.Create(-1, ExpressTokenType.String, ((DateTime)json.Value).ToString("O"));
                case STJsonValueType.Array:
                    arr_obj = new ExpressionToken[json.Count];
                    for (int i = 0; i < json.Count; i++) {
                        arr_obj[i] = ME.GetExpressTokenFromSTJson(json[i]);
                    }
                    return ExpressionToken.Create(-1, ExpressTokenType.Array, arr_obj);
            }
            return ExpressionToken.Create(-1, ExpressTokenType.Object, json);
        }

        private static ExpressionToken ExcuteArray(ExpressionToken token, STJson jsonRoot, STJson jsonCurrent) {
            STJsonPathExpression[] src = token.Value as STJsonPathExpression[];
            ExpressionToken[] result = new ExpressionToken[src.Length];
            for (int i = 0; i < src.Length; i++) {
                result[i] = src[i].Excute(jsonRoot, jsonCurrent);
            }
            return ExpressionToken.Create(-1, ExpressTokenType.Array, result);
        }

        private static ExpressionToken ExcuteFunc(ExpressionToken exp, STJson jsonRoot, STJson jsonCurrent) {
            var arr_exp_arg = new ExpressionToken[exp.Args.Count];
            for (int i = 0; i < exp.Args.Count; i++) {
                arr_exp_arg[i] = exp.Args[i].Excute(jsonRoot, jsonCurrent);
            }
            string strFuncName = exp.Value.ToString();
            if (STJsonPath.CustomFunctions.ContainsKey(strFuncName)) {
                var arr_custom_arg = new STJsonPathExpFuncArg[exp.Args.Count];
                for (int i = 0; i < arr_exp_arg.Length; i++) {
                    arr_custom_arg[i] = ME.ConvertExpressTokenToFuncArg(arr_exp_arg[i]);
                }
                var json = STJsonPath.CustomFunctions[strFuncName](arr_custom_arg);
                if (exp.PathItems != null && exp.PathItems.Count != 0) {
                    return ME.GetExpressTokenFromPathItems(exp, json, json);
                }
                return ME.GetExpressTokenFromSTJson(json);
            }
            if (STJsonPath.BuildInFunctions.ContainsKey(strFuncName)) {
                return STJsonPath.BuildInFunctions[strFuncName](arr_exp_arg);
            }
            return ExpressionToken.Undefined;
            //throw new STJsonPathExpressException(1, "Can not found the function [" + strFuncName + "]");
        }


        private static STJsonPathExpFuncArg ConvertExpressTokenToFuncArg(ExpressionToken token) {
            STJsonPathExpFuncArg arg = new STJsonPathExpFuncArg();
            if (token.Type == ExpressTokenType.Array) {
                var tokens = token.Value as ExpressionToken[];
                STJsonPathExpFuncArg[] arr_arg = new STJsonPathExpFuncArg[tokens.Length];
                arg.Type = STJsonPathExpFuncArgType.Array;
                for (int i = 0; i < arr_arg.Length; i++) {
                    arr_arg[i] = ME.ConvertExpressTokenToFuncArg(tokens[i]);
                }
                arg.Value = arr_arg;
                return arg;
            }
            if (token.Type <= ExpressTokenType.Object) {
                arg.Type = (STJsonPathExpFuncArgType)token.Type;
                arg.Value = token.Value;
            }
            return arg;
        }

        private static ExpressionToken ExcuteBasic(ExpressionToken l, ExpressionToken r, string strOP) {
            if (l.Type == ExpressTokenType.String || r.Type == ExpressTokenType.String) {
                if (strOP == "+") {
                    return ExpressionToken.Create(
                        -1,
                        ExpressTokenType.String,
                        Convert.ToString(l.Value) + Convert.ToString(r.Value)
                        );
                }
                return ExpressionToken.Undefined;
                //throw new STJsonPathExpressException(0, "The data types do not match and this operation cannot be completed. {" + l.Type + " " + strOP + " " + r.Type + "}");
            }
            ExpressionToken token_ret = new ExpressionToken() {
                Index = -1,
                Type = (l.Type == ExpressTokenType.Double || r.Type == ExpressTokenType.Double)
                ? ExpressTokenType.Double
                : ExpressTokenType.Long
            };
            if (token_ret.Type == ExpressTokenType.Double) {
                double d_l = Convert.ToDouble(l.Value);
                double d_r = Convert.ToDouble(r.Value);
                switch (strOP) {
                    case "+": token_ret.Value = d_l + d_r; break;
                    case "-": token_ret.Value = d_l - d_r; break;
                    case "*": token_ret.Value = d_l * d_r; break;
                    case "/": token_ret.Value = d_l / d_r; break;
                    case "%":
                        return ExpressionToken.Undefined;
                        //throw new STJsonPathExpressException(0, "The data types do not match and this operation cannot be completed. {" + l.Type + " " + strOP + " " + r.Type + "}");
                }
            } else {
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

        private static ExpressionToken ExcuteBit(ExpressionToken l, ExpressionToken r, string strOP) {
            if (l.Type != ExpressTokenType.Long || r.Type != ExpressTokenType.Long) {
                return ExpressionToken.Undefined;
                //throw new STJsonPathExpressException(0, "The data types do not match and this operation cannot be completed. {" + l.Type + " " + strOP + " " + r.Type + "}");
            }
            ExpressionToken token_ret = new ExpressionToken() {
                Type = ExpressTokenType.Long
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

        private static ExpressionToken ExcuteLogic(ExpressionToken l, ExpressionToken r, string strOP) {
            ExpressionToken token_ret = new ExpressionToken() {
                Index = -1,
                Type = ExpressTokenType.Boolean
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
                return ExpressionToken.False;
            }
            if (l.Type == ExpressTokenType.Double || r.Type == ExpressTokenType.Double) {
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

        private static ExpressionToken ExcuteList(ExpressionToken l, ExpressionToken r, string strOP) {
            if (r.Type != ExpressTokenType.Array) {
                return ExpressionToken.False;
            }
            ExpressionToken[] arr_l = null;
            ExpressionToken[] arr_r = r.Value as ExpressionToken[];
            if (l.Type == ExpressTokenType.Array) {
                arr_l = l.Value as ExpressionToken[];
            } else {
                arr_l = new ExpressionToken[] { l };
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
                    return nCounter == arr_l.Length ? ExpressionToken.True : ExpressionToken.False;
                case "nin":
                    foreach (var obj_l in arr_l) {
                        foreach (var obj_r in arr_r) {
                            if (object.Equals(obj_l.Value, obj_r.Value)) {
                                return ExpressionToken.False;
                            }
                        }
                    }
                    return ExpressionToken.True;
                case "anyof":
                    foreach (var obj_l in arr_l) {
                        foreach (var obj_r in arr_r) {
                            if (object.Equals(obj_l.Value, obj_r.Value)) {
                                return ExpressionToken.True;
                            }
                        }
                    }
                    return ExpressionToken.False;
            }
            return ExpressionToken.False;
        }

        // =====================================================================================================

        private static ExpressionToken ExcuteTest(List<ExpressionToken> tokens, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut) {
            ExpressionToken token_l = ExpressionToken.Undefined;
            ExpressionToken token_r = ExpressionToken.Undefined;
            ExpressionToken token_result = ExpressionToken.Undefined;
            Stack<ExpressionToken> stack_result = new Stack<ExpressionToken>();
            STJson jsonStep = null;
            STJson jsonSteps = STJson.CreateArray();
            jsonOut.SetItem("steps", jsonSteps);
            foreach (var obj in tokens) {
                switch (obj.Type) {
                    case ExpressTokenType.Operator:
                        jsonStep = new STJson();
                        jsonStep.SetItem("type", "excute");
                        jsonStep.SetItem("operator", obj.Value);
                        jsonSteps.Append(jsonStep);
                        token_r = stack_result.Pop();
                        token_l = stack_result.Pop();
                        token_l = ME.GetExpressTokenTest(token_l, jsonRoot, jsonCurrent, jsonStep.SetKey("get_left_token"));
                        token_r = ME.GetExpressTokenTest(token_r, jsonRoot, jsonCurrent, jsonStep.SetKey("get_right_token"));
                        if (token_l.Type == ExpressTokenType.Undefined || token_r.Type == ExpressTokenType.Undefined) {
                            return ExpressionToken.Undefined;
                        }
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
                return ExpressionToken.Undefined;
            }
            var ret = stack_result.Pop();
            return ME.GetExpressTokenTest(ret, jsonRoot, jsonCurrent, jsonOut.SetKey("check_result"));
        }

        private static ExpressionToken GetExpressTokenTest(ExpressionToken token, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut) {
            jsonOut.SetItem("parsed", token.Text);
            switch (token.Type) {
                case ExpressTokenType.PathItem:
                    jsonOut.SetItem("type", "selector");
                    token = ME.GetExpressTokenFromPathItemsTest(token, jsonRoot, jsonCurrent, jsonOut);
                    break;
                case ExpressTokenType.Func:
                    jsonOut.SetItem("type", "function");
                    token = STJsonPathExpression.ExcuteFuncTest(token, jsonRoot, jsonCurrent, jsonOut);
                    break;
                case ExpressTokenType.ArrayExp:
                    jsonOut.SetItem("type", "array_expression");
                    token = ME.ExcuteArrayTest(token, jsonRoot, jsonCurrent, jsonOut);
                    break;
                default:
                    jsonOut.SetItem("type", "value");
                    break;
            }
            jsonOut.SetItem("result", new STJson().SetItem("value_type", token.Type.ToString()).SetItem("text", token.Text));
            return token;
        }

        private static ExpressionToken GetExpressTokenFromPathItemsTest(ExpressionToken token, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut) {
            var json_result = STJson.CreateArray();
            SelectSetting setting = new SelectSetting() {
                Root = jsonRoot,
                Mode = STJsonPathSelectMode.ItemOnly
            };
            STJsonPath.GetSTJsons(token.PathItems, 0, jsonCurrent, setting, json_result);
            jsonOut.SetItem("root_json", jsonRoot == null ? null : jsonRoot.ToString());
            jsonOut.SetItem("current_json", jsonCurrent == null ? null : jsonCurrent.ToString());
            jsonOut.SetItem("selected_json", json_result.ToString());
            if (json_result.Count == 0) {
                return ExpressionToken.Undefined;
            }
            var token_ret = ME.GetExpressTokenFromSTJson(json_result[0]);
            return token_ret;
        }

        private static ExpressionToken ExcuteArrayTest(ExpressionToken token, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut) {
            var jsonItems = STJson.CreateArray();
            jsonOut.SetItem("get_items", jsonItems);
            STJsonPathExpression[] src = token.Value as STJsonPathExpression[];
            ExpressionToken[] result = new ExpressionToken[src.Length];
            for (int i = 0; i < src.Length; i++) {
                var jsonItem = new STJson();
                result[i] = src[i].ExcuteTest(jsonRoot, jsonCurrent, jsonItem);
                jsonItems.Append(jsonItem);
            }
            return ExpressionToken.Create(-1, ExpressTokenType.Array, result);
        }

        private static ExpressionToken ExcuteFuncTest(ExpressionToken exp, STJson jsonRoot, STJson jsonCurrent, STJson jsonOut) {
            var token_return = ExpressionToken.Undefined;
            var jsonArgs = STJson.CreateArray();
            List<object> lst_args = new List<object>();
            jsonOut.SetItem("get_args", jsonArgs);
            var arr_exp_arg = new ExpressionToken[exp.Args.Count];
            for (int i = 0; i < exp.Args.Count; i++) {
                var jsonArg = new STJson();
                arr_exp_arg[i] = exp.Args[i].ExcuteTest(jsonRoot, jsonCurrent, jsonArg);
                jsonArgs.Append(jsonArg);
                lst_args.Add(arr_exp_arg[i].Text);
            }
            jsonOut.SetItem("args_text", lst_args);
            string strFuncName = exp.Value.ToString();
            if (STJsonPath.CustomFunctions.ContainsKey(strFuncName)) {
                jsonOut.SetItem("type", "custom_function");
                var arr_custom_arg = new STJsonPathExpFuncArg[exp.Args.Count];
                for (int i = 0; i < arr_exp_arg.Length; i++) {
                    arr_custom_arg[i] = ME.ConvertExpressTokenToFuncArg(arr_exp_arg[i]);
                }
                var json = STJsonPath.CustomFunctions[strFuncName](arr_custom_arg);
                jsonOut.SetItem("result_json", json.ToString());
                if (exp.PathItems != null && exp.PathItems.Count != 0) {
                    token_return = ME.GetExpressTokenFromPathItemsTest(exp, json, json, jsonOut);
                } else {
                    token_return = ME.GetExpressTokenFromSTJson(json);
                }
                return token_return;
            }
            if (STJsonPath.BuildInFunctions.ContainsKey(strFuncName)) {
                jsonOut.SetItem("type", "buildin_function");
                return STJsonPath.BuildInFunctions[strFuncName](arr_exp_arg);
            }
            jsonOut.SetItem("error", "Can not found the function [" + strFuncName + "]");
            return ExpressionToken.Undefined;
        }
    }
}
