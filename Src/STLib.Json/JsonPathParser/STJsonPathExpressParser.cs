using System;
using System.Collections.Generic;
using System.Linq;

using ME = STLib.Json.STJsonPathExpressParser;

namespace STLib.Json
{
    internal class STJsonPathExpressParser
    {

        private static Dictionary<string, int> m_dic_op_priority = new Dictionary<string, int>();

        private static string[] m_strs_priority = new string[] {
            "&&,||",
            "<,<=,>,>=,==,!=,re",           // re: regexpression
            "&,|,<<,>>,^,~",
            "+,-",
            "*,/,%",
            "in,nin,anyof",
            "!"
        };

        static STJsonPathExpressParser() {
            for (int i = 0; i < m_strs_priority.Length; i++) {
                foreach (var v in m_strs_priority[i].Split(',')) {
                    m_dic_op_priority.Add(v, i);
                }
            }
        }

        public static STJsonPathExpression GetSTJsonPathExpress(List<STJsonPathToken> tokens, bool isFilter) {
            string strText = string.Empty;
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                if (token.Type != STJsonPathTokenType.String) {
                    if (m_dic_op_priority.ContainsKey(token.Value)) {
                        token.Type = STJsonPathTokenType.Operator;
                    }
                }
                tokens[i] = token;
            }
            var lst_exp_token = ME.GetExpressTokens(tokens);
            foreach (var v in lst_exp_token) {
                if (v.Type == ExpressTokenType.String) {
                    strText += "'" + STJson.Escape(v.Text) + "'";
                } else {
                    strText += v.Text;
                }
            }
            ME.CheckExpress(lst_exp_token);
            var lst = ME.GetExpressPolishQueue(lst_exp_token);
            return new STJsonPathExpression("{" + strText + "}", lst, isFilter);
        }

        public static List<ExpressionToken> GetExpressTokens(List<STJsonPathToken> tokens) {
            STJsonPathToken token_next = new STJsonPathToken();
            List<ExpressionToken> lst_result = new List<ExpressionToken>();
            List<STJsonPathToken> lst_range = null;
            List<STJsonPathToken> lst_selector_tokens = new List<STJsonPathToken>();
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                if (m_dic_op_priority.ContainsKey(token.Value)) {
                    lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.Operator, token.Value));
                    continue;
                }
                switch (token.Type) {
                    case STJsonPathTokenType.Long:
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.Long, long.Parse(token.Value)));
                        continue;
                    case STJsonPathTokenType.Double:
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.Double, double.Parse(token.Value)));
                        continue;
                    case STJsonPathTokenType.String:
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.String, token.Value));
                        continue;
                    case STJsonPathTokenType.Property:
                        if (i + 1 < tokens.Count) {
                            token_next = tokens[i + 1];
                            if (token_next.IsSymbol("(")) {
                                lst_result.Add(ME.GetFuncToken(tokens, ref i));
                                i--;
                                continue;
                            }
                        }
                        break;
                }
                if (token.Value == "[") {
                    lst_range = STJsonPathTokenizer.GetRange(tokens, i, "[", "]");
                    if (lst_range.Count < 1) {
                        throw new STJsonPathParseException(token.Index, "The bracket is empty. index: " + token.Index);
                    }
                    lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.ArrayExp, ME.GetExpressesFromRange(lst_range).ToArray()));
                    i += lst_range.Count + 1;
                    continue;
                }
                switch (token.Value) {
                    case "(":
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.LeftParentheses, "("));
                        continue;
                    case ")":
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.RightParentheses, ")"));
                        continue;
                    case "true":
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.Boolean, true));
                        continue;
                    case "false":
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.Boolean, false));
                        continue;
                    case "null":
                        lst_result.Add(ExpressionToken.Create(token.Index, ExpressTokenType.Object, null));
                        continue;
                    case "$":
                    case "@":
                        lst_selector_tokens.Clear();
                        while (i < tokens.Count) {
                            if (tokens[i].IsSymbol("[")) {
                                lst_range = STJsonPathTokenizer.GetRange(tokens, i, "[", "]");
                                if (lst_range.Count < 1) {
                                    throw new STJsonPathParseException(tokens[i].Index, "The bracket is empty. index: " + token.Index);
                                }
                                lst_selector_tokens.Add(tokens[i]);
                                lst_selector_tokens.AddRange(lst_range);
                                lst_selector_tokens.Add(tokens[i + lst_range.Count + 1]);
                                i += lst_range.Count + 2;
                                continue;
                            }
                            if (tokens[i].Type == STJsonPathTokenType.Operator) {
                                if (tokens[i].Value != "*" || i == 0 || tokens[i - 1].Value != ".") { // for case : @.X.*
                                    break;
                                }
                            }
                            lst_selector_tokens.Add(tokens[i++]);
                        }
                        i--;
                        lst_result.Add(ExpressionToken.Create(token.Index, STJsonPathParser.GetPathItems(lst_selector_tokens)));
                        continue;
                    default:
                        throw new STJsonPathParseException(token.Index, "Unknows token [" + token.Value + "]");
                }
            }
            return lst_result;
        }

        private static ExpressionToken GetFuncToken(List<STJsonPathToken> tokens, ref int nIndex) {
            var strFuncName = tokens[nIndex].Value;
            var nFirstIndex = nIndex;
            var lst_range = STJsonPathTokenizer.GetRange(tokens, nIndex + 1, "(", ")");
            var lst_exp_arg = lst_range.Count == 0 ? new List<STJsonPathExpression>() : ME.GetExpressesFromRange(lst_range);
            nIndex += lst_range.Count + 3;
            if (nIndex >= tokens.Count || tokens[nIndex].Type == STJsonPathTokenType.Operator) {
                return ExpressionToken.Create(nFirstIndex, strFuncName, lst_exp_arg, null);
            }
            int nA = 0, nB = 0;
            List<STJsonPathToken> lst_token_temp = new List<STJsonPathToken>();
            //lst_token_temp.Add(new STJsonPathToken(nIndex, "@", STJsonPathTokenType.None));
            for (int i = nIndex; i < tokens.Count; i++, nIndex = i) {
                var t = tokens[i];
                if (t.Type != STJsonPathTokenType.String) {
                    switch (t.Value) {
                        case "(":
                            if (t.Type != STJsonPathTokenType.String) {
                                nA++;
                            }
                            break;
                        case ")":
                            if (t.Type != STJsonPathTokenType.String) {
                                nA--;
                            }
                            break;
                        case "[":
                            if (t.Type != STJsonPathTokenType.String) {
                                nB++;
                            }
                            break;
                        case "]":
                            if (t.Type != STJsonPathTokenType.String) {
                                nB--;
                            }
                            break;
                    }
                    if (t.Type == STJsonPathTokenType.Operator) {
                        if (nA == 0 && nB == 0) {
                            break;
                        }
                    }
                }
                lst_token_temp.Add(tokens[i]);
            }
            var items = STJsonPathParser.GetPathItems(lst_token_temp);
            return ExpressionToken.Create(nFirstIndex, strFuncName, lst_exp_arg, items);
        }

        public static List<STJsonPathExpression> GetExpressesFromRange(List<STJsonPathToken> tokens) {
            int nA = 0, nB = 0;
            var lst_token_temp = new List<STJsonPathToken>();
            var lst_exp_ret = new List<STJsonPathExpression>();
            foreach (var t in tokens) {
                if (t.Type != STJsonPathTokenType.String) {
                    switch (t.Value) {
                        case "(":
                            if (t.Type != STJsonPathTokenType.String) {
                                nA++;
                            }
                            break;
                        case ")":
                            if (t.Type != STJsonPathTokenType.String) {
                                nA--;
                            }
                            break;
                        case "[":
                            if (t.Type != STJsonPathTokenType.String) {
                                nB++;
                            }
                            break;
                        case "]":
                            if (t.Type != STJsonPathTokenType.String) {
                                nB--;
                            }
                            break;
                        case ",":
                            if (nA != 0 || nB != 0) {
                                break;
                            }
                            if (lst_token_temp.Count == 0) {
                                throw new STJsonPathParseException(t.Index, "Unknows token [" + t.Value + "]");
                            }
                            lst_exp_ret.Add(ME.GetSTJsonPathExpress(lst_token_temp, false));
                            lst_token_temp.Clear();
                            continue;
                    }
                }
                lst_token_temp.Add(t);
            }
            if (lst_token_temp.Count != 0) {
                lst_exp_ret.Add(ME.GetSTJsonPathExpress(lst_token_temp, false));
            }
            return lst_exp_ret;
        }

        public static List<ExpressionToken> GetExpressPolishQueue(List<ExpressionToken> tokens) {
            Stack<ExpressionToken> stack_sop = new Stack<ExpressionToken>();
            List<ExpressionToken> lst_out = new List<ExpressionToken>();
            ExpressionToken sop_top = ExpressionToken.Undefined;
            string str_last_op = string.Empty;
            foreach (var token in tokens) {
                switch (token.Type) {
                    case ExpressTokenType.LeftParentheses:
                        stack_sop.Push(token);
                        continue;
                    case ExpressTokenType.RightParentheses:
                        while (true) {
                            if (stack_sop.Count == 0) {
                                throw new STJsonPathParseException(token.Index, "Invalid express [" + ME.GetExpressText(tokens) + "]");
                            }
                            sop_top = stack_sop.Pop();
                            if (sop_top.Type == ExpressTokenType.LeftParentheses) {
                                break;
                            }
                            if (lst_out.Count < 2) {
                                throw new STJsonPathParseException(token.Index, "Invalid express [" + ME.GetExpressText(tokens) + "]");
                            }
                            lst_out.Add(sop_top);
                        }
                        continue;
                    case ExpressTokenType.Operator:
                        str_last_op = token.Value.ToString();
                        while (stack_sop.Count > 0) {
                            sop_top = stack_sop.First();
                            if (sop_top.Type == ExpressTokenType.LeftParentheses) {
                                break;
                            }
                            if (m_dic_op_priority[token.Value.ToString()] > m_dic_op_priority[sop_top.Value.ToString()]) {
                                break;
                            }
                            lst_out.Add(stack_sop.Pop());
                        }
                        stack_sop.Push(token);
                        continue;
                    default:
                        lst_out.Add(token);
                        continue;
                }
            }
            while (stack_sop.Count != 0) lst_out.Add(stack_sop.Pop());
            return lst_out;
        }

        private static void CheckExpress(List<ExpressionToken> tokens) {
            switch (tokens[0].Type) {
                case ExpressTokenType.RightParentheses:
                case ExpressTokenType.Operator:
                    switch (tokens[0].Value) {
                        case "!":
                        case "~":
                            break;
                        default:
                            throw new STJsonPathParseException(tokens[0].Index, "Invalid expression {" + ME.GetExpressText(tokens) + "}");
                    }
                    break;
            }
            if (tokens[tokens.Count - 1].Type == ExpressTokenType.Operator) {
                throw new STJsonPathParseException(tokens[tokens.Count - 1].Index, "Invalid expression {" + ME.GetExpressText(tokens) + "}");
            }
            double d_v = 0;
            long l_v = 0;
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                switch (token.Type) {
                    case ExpressTokenType.Long:
                        l_v = Convert.ToInt64(token.Value);
                        if (l_v < 0 && i != 0 && tokens[i - 1].Type != ExpressTokenType.Operator) {
                            token.Value = -l_v;
                            tokens[i] = token;
                            tokens.Insert(i, ExpressionToken.Create(-1, ExpressTokenType.Operator, "-"));
                            i++;
                        }
                        break;
                    case ExpressTokenType.Double:
                        d_v = Convert.ToDouble(token.Value);
                        if (d_v < 0 && i != 0 && tokens[i - 1].Type != ExpressTokenType.Operator) {
                            token.Value = -d_v;
                            tokens[i] = token;
                            tokens.Insert(i, ExpressionToken.Create(-1, ExpressTokenType.Operator, "-"));
                            i++;
                        }
                        break;
                    case ExpressTokenType.Operator:
                        switch (token.Value.ToString()) {
                            case "!":
                            case "~":
                                tokens.Insert(i, ExpressionToken.Create(-1, ExpressTokenType.Long, 0));
                                i++;
                                break;
                        }
                        break;
                }
            }
            int nA = 0;
            bool bNeedOP = false;
            var token_last = new ExpressionToken();
            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                switch (token.Type) {
                    case ExpressTokenType.LeftParentheses:
                        switch (token_last.Type) {
                            case ExpressTokenType.Undefined:
                            case ExpressTokenType.LeftParentheses:
                            case ExpressTokenType.Operator:
                                break;
                                throw new STJsonPathParseException(token.Index, "Invalid expression {" + ME.GetExpressText(tokens) + "}");
                        }
                        nA++;
                        bNeedOP = false;
                        break;
                    case ExpressTokenType.RightParentheses:
                        switch (token_last.Type) {
                            case ExpressTokenType.RightParentheses:
                                break;
                            case ExpressTokenType.Operator:
                                throw new STJsonPathParseException(token.Index, "Invalid expression {" + ME.GetExpressText(tokens) + "}");
                        }
                        nA--;
                        if (nA < 0) {
                            throw new STJsonPathParseException(token.Index, "Invalid expression {" + ME.GetExpressText(tokens) + "}");
                        }
                        bNeedOP = true;
                        break;
                    case ExpressTokenType.Operator:
                        if (!bNeedOP) {
                            throw new STJsonPathParseException(token.Index, "Invalid expression {" + ME.GetExpressText(tokens) + "}");
                        }
                        bNeedOP = false;
                        break;
                    default:
                        if (bNeedOP) {
                            throw new STJsonPathParseException(token.Index, "Invalid expression {" + ME.GetExpressText(tokens) + "}");
                        }
                        bNeedOP = true;
                        break;
                }
                token_last = token;
            }

        }

        private static string GetExpressText(List<ExpressionToken> tokens) {
            string strText = string.Empty;
            foreach (var v in tokens) {
                strText += v.Text;
            }
            return strText;
        }
    }
}
