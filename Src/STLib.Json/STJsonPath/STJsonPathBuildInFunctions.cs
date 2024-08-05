using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ME = STLib.Json.STJsonPathBuildInFunctions;

namespace STLib.Json
{
    internal class STJsonPathBuildInFunctions
    {
        private delegate STJsonPathExpressionToken GeneralCallback(STJsonPathExpressionToken token);

        private static List<FNInfo> m_lst_info;

        public static STJson FuncList { get; private set; }

        [STJson(STJsonSerializeMode.Exclude)]
        private struct FNInfo
        {
            public string name;
            public string[] demos;
            [STJsonProperty]
            public STJsonPathBuildInFuncHander func;

            public FNInfo(string strName, STJsonPathBuildInFuncHander fn, string[] demos)
            {
                this.name = strName;
                this.func = fn;
                this.demos = demos;
            }
        }

        public static void Init()
        {
            m_lst_info = new List<FNInfo>();
            m_lst_info.Add(new FNInfo("typeof", ME.FN_typeof, "(object) -> typeof('abc')".Split('|')));
            m_lst_info.Add(new FNInfo("str", ME.FN_str, "(object) -> str(123)".Split('|')));
            m_lst_info.Add(new FNInfo("upper", ME.FN_upper, "(string) -> upper('abc')".Split('|')));
            m_lst_info.Add(new FNInfo("lower", ME.FN_lower, "(string) -> lower('abc')".Split('|')));

            m_lst_info.Add(new FNInfo("len", ME.FN_len, "(string|array) -> len('abc')".Split('|')));
            m_lst_info.Add(new FNInfo("long", ME.FN_long, "(object) -> long('123')".Split('|')));
            m_lst_info.Add(new FNInfo("double", ME.FN_double, "(object) -> double('123')".Split('|')));
            m_lst_info.Add(new FNInfo("bool", ME.FN_bool, "(object) -> bool('abc')".Split('|')));

            m_lst_info.Add(new FNInfo("abs", ME.FN_abs, "(number) -> abs(-123)".Split('|')));
            m_lst_info.Add(new FNInfo("round", ME.FN_round, "(number) -> round(123.4)".Split('|')));
            m_lst_info.Add(new FNInfo("ceil", ME.FN_lower, "(number) -> ceil(123.4)".Split('|')));

            m_lst_info.Add(new FNInfo("max", ME.FN_max, "(array) -> max([1,2,3,4,5])".Split('|')));
            m_lst_info.Add(new FNInfo("min", ME.FN_min, "(array) -> max([1,2,3,4,5])".Split('|')));
            m_lst_info.Add(new FNInfo("avg", ME.FN_avg, "(array) -> max([1,2,3,4,5])".Split('|')));
            m_lst_info.Add(new FNInfo("sum", ME.FN_sum, "(array) -> max([1,2,3,4,5])".Split('|')));

            m_lst_info.Add(new FNInfo("trim", ME.FN_trim, "(string) -> trim('abc')|(string,string) -> trim(',.abc.,','.,')".Split('|')));
            m_lst_info.Add(new FNInfo("trims", ME.FN_trims, "(string) -> trims('abc')|(string,string) -> trims(',.abc.,','.,')".Split('|')));
            m_lst_info.Add(new FNInfo("trime", ME.FN_trime, "(string) -> trime('abc')|(string,string) -> trime(',.abc.,','.,')".Split('|')));
            m_lst_info.Add(new FNInfo("split", ME.FN_split, @"(string,regex_string) -> split('a.b.c','\\.')".Split('|')));

            m_lst_info.Add(new FNInfo("time", ME.FN_time, "() -> time()|(string) -> time('yyyy-MM-dd')|(long,string) -> time(1684751411726,'yyyy-MM-dd')".Split('|')));

            foreach (var v in m_lst_info) {
                STJsonPath.BuildInFunctions.Add(v.name, v.func);
            }

            ME.FuncList = STJson.FromObject(m_lst_info, STJsonSetting.Default);
        }

        public static STJsonPathExpressionToken FN_typeof(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: typeof(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_typeof(args[0]);
        }

        private static STJsonPathExpressionToken FN_typeof(STJsonPathExpressionToken token)
        {
            switch (token.Type) {
                case STJsonPathExpressionTokenType.String: return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, "string");
                case STJsonPathExpressionTokenType.Long: return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, "long");
                case STJsonPathExpressionTokenType.Double: return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, "double");
                case STJsonPathExpressionTokenType.Boolean: return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, "boolean");
                case STJsonPathExpressionTokenType.Array: return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, "array");
                case STJsonPathExpressionTokenType.Object: return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, "object");
            }
            return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, "unknows");
        }

        public static STJsonPathExpressionToken FN_str(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: str(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_str(args[0]);
        }

        private static STJsonPathExpressionToken FN_str(STJsonPathExpressionToken token)
        {
            return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, token.Text);
        }

        public static STJsonPathExpressionToken FN_upper(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: upper(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_upper(args[0]);
        }

        private static STJsonPathExpressionToken FN_upper(STJsonPathExpressionToken token)
        {
            if (token.Value == null) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, null);
            }
            switch (token.Type) {
                case STJsonPathExpressionTokenType.String:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, Convert.ToString(token.Value).ToUpper());
            }
            return STJsonPathExpressionToken.CreateError("Can not match the function: upper(" + token.Type + "). ");
        }

        public static STJsonPathExpressionToken FN_lower(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: lower(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_lower(args[0]);
        }

        private static STJsonPathExpressionToken FN_lower(STJsonPathExpressionToken token)
        {
            if (token.Value == null) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, null);
            }
            switch (token.Type) {
                case STJsonPathExpressionTokenType.String:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, Convert.ToString(token.Value).ToLower());
            }
            return STJsonPathExpressionToken.CreateError("Can not match the function: lower(" + token.Type + "). ");
        }

        public static STJsonPathExpressionToken FN_len(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: len(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_len(args[0]);
        }

        private static STJsonPathExpressionToken FN_len(STJsonPathExpressionToken token)
        {
            if (token.Value == null) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, 0);
            }
            switch (token.Type) {
                case STJsonPathExpressionTokenType.String:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, Convert.ToString(token.Value).Length);
                case STJsonPathExpressionTokenType.Array:
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, (token.Value as STJsonPathExpressionToken[]).Length);
            }
            return STJsonPathExpressionToken.CreateError("Can not match the function: len(" + token.Type + "). ");
        }

        public static STJsonPathExpressionToken FN_long(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: long(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_long(args[0]);
        }

        private static STJsonPathExpressionToken FN_long(STJsonPathExpressionToken token)
        {
            if (token.Value == null) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, 0);
            }
            try {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, Convert.ToInt64(token.Value));
            } catch (Exception ex) {
                return STJsonPathExpressionToken.CreateError("Get an error when excute function: long(" + token.Type + "). " + ex.Message);
            }
        }

        public static STJsonPathExpressionToken FN_double(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: double(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_double(args[0]);
        }

        private static STJsonPathExpressionToken FN_double(STJsonPathExpressionToken token)
        {
            if (token.Value == null) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Double, 0);
            }
            try {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, Convert.ToDouble(token.Value));
            } catch (Exception ex) {
                return STJsonPathExpressionToken.CreateError("Get an error when excute function: double(" + token.Type + "). " + ex.Message);
            }
        }

        public static STJsonPathExpressionToken FN_bool(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: bool(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_bool(args[0]);
        }

        private static STJsonPathExpressionToken FN_bool(STJsonPathExpressionToken token)
        {
            try {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Boolean, Convert.ToBoolean(token.Value));
            } catch {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Boolean, false);
            }
        }

        public static STJsonPathExpressionToken FN_abs(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: abs(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_abs(args[0]);
        }

        private static STJsonPathExpressionToken FN_abs(STJsonPathExpressionToken token)
        {
            long l_v = 0;
            double d_v = 0;
            switch (token.Type) {
                case STJsonPathExpressionTokenType.Long:
                    l_v = Convert.ToInt64(token.Value);
                    if (l_v < 0) {
                        token.Value = -l_v;
                    }
                    return token;
                case STJsonPathExpressionTokenType.Double:
                    d_v = Convert.ToDouble(token.Value);
                    if (d_v < 0) {
                        token.Value = -d_v;
                    }
                    return token;
                default:
                    return STJsonPathExpressionToken.CreateError("Can not match the function: abs(" + token.Type + "). ");
            }
        }

        public static STJsonPathExpressionToken FN_round(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: round(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_round(args[0]);
        }

        private static STJsonPathExpressionToken FN_round(STJsonPathExpressionToken token)
        {
            switch (token.Type) {
                case STJsonPathExpressionTokenType.Long: return token;
                case STJsonPathExpressionTokenType.Double:
                    token.Type = STJsonPathExpressionTokenType.Long;
                    token.Value = (long)Math.Round(Convert.ToDouble(token.Value));
                    return token;
                default:
                    return STJsonPathExpressionToken.CreateError("Can not match the function: round(" + token.Type + "). ");
            }
        }

        public static STJsonPathExpressionToken FN_ceil(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: ceil(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            return ME.FN_ceil(args[0]);
        }

        private static STJsonPathExpressionToken FN_ceil(STJsonPathExpressionToken token)
        {
            switch (token.Type) {
                case STJsonPathExpressionTokenType.Long: return token;
                case STJsonPathExpressionTokenType.Double:
                    token.Type = STJsonPathExpressionTokenType.Long;
                    token.Value = (long)Math.Ceiling(Convert.ToDouble(token.Value));
                    return token;
                default:
                    return STJsonPathExpressionToken.CreateError("Can not match the function: ceil(" + token.Type + "). ");
            }
        }


        private static STJsonPathExpressionToken FN_max(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1 || args[0].Type != STJsonPathExpressionTokenType.Array) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: max(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            STJsonPathExpressionToken token = STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Double, double.MinValue);
            double d_v = double.MinValue, d_t = 0;
            foreach (var v in args[0].Value as STJsonPathExpressionToken[]) {
                if (!v.IsNumber) {
                    continue;
                }
                d_t = Convert.ToDouble(v.Value);
                if (d_t > d_v) {
                    token = v;
                    d_v = d_t;
                }
            }
            return token;
        }

        private static STJsonPathExpressionToken FN_min(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1 || args[0].Type != STJsonPathExpressionTokenType.Array) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: min(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            STJsonPathExpressionToken token = STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Double, double.MaxValue);
            double d_v = double.MaxValue, d_t = 0;
            foreach (var v in args[0].Value as STJsonPathExpressionToken[]) {
                if (!v.IsNumber) {
                    continue;
                }
                d_t = Convert.ToDouble(v.Value);
                if (d_t < d_v) {
                    token = v;
                    d_v = d_t;
                }
            }
            return token;
        }

        private static STJsonPathExpressionToken FN_avg(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1 || args[0].Type != STJsonPathExpressionTokenType.Array) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: avg(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            double d_v = 0;
            int nCounter = 0;
            foreach (var v in args[0].Value as STJsonPathExpressionToken[]) {
                if (!v.IsNumber) {
                    continue;
                }
                nCounter++;
                d_v += Convert.ToDouble(v.Value);
            }
            if (nCounter == 0) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Double, 0);
            }
            return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Double, d_v / nCounter);
        }

        private static STJsonPathExpressionToken FN_sum(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length != 1 || args[0].Type != STJsonPathExpressionTokenType.Array) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: sum(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            double d_v = 0;
            foreach (var v in args[0].Value as STJsonPathExpressionToken[]) {
                if (!v.IsNumber) {
                    continue;
                }
                d_v += Convert.ToDouble(v.Value);
            }
            return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Double, d_v);
        }


        public static STJsonPathExpressionToken FN_split(STJsonPathExpressionToken[] args)
        {
            bool b_flag = 
                args == null || 
                args.Length != 2 || 
                args[0].Type != STJsonPathExpressionTokenType.String ||
                args[2].Type != STJsonPathExpressionTokenType.String;
            if (b_flag) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: abs(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            try {
                Regex reg = new Regex(args[1].Value.ToString());
                var strs = reg.Split(args[0].Value.ToString());
                var results = new STJsonPathExpressionToken[strs.Length];
                for (int i = 0; i < strs.Length; i++) {
                    results[i] = STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, strs[i]);
                }
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Array, results);
            } catch (Exception ex) {
                return STJsonPathExpressionToken.CreateError("Get an error when excute function: split(" + ME.GetFunctionArgTypeList(args) + "). " + ex.Message);
            }
        }

        public static STJsonPathExpressionToken FN_trim(STJsonPathExpressionToken[] args)
        {
            return ME.FN_trim(args, 0);
        }

        private static STJsonPathExpressionToken FN_trims(STJsonPathExpressionToken[] args)
        {
            return ME.FN_trim(args, 1);
        }

        private static STJsonPathExpressionToken FN_trime(STJsonPathExpressionToken[] args)
        {
            return ME.FN_trim(args, 2);
        }

        private static STJsonPathExpressionToken FN_trim(STJsonPathExpressionToken[] args, int nModel)
        {
            if (args == null || args.Length < 1) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, null);
            }
            if (args[0].Type != STJsonPathExpressionTokenType.String) {
                return STJsonPathExpressionToken.CreateError("Can not match the function: trim*(" + ME.GetFunctionArgTypeList(args) + "). ");
            }
            char[] arr_ch_trim = new char[] { ' ', '\t', '\r', '\n' }; ;
            try {
                if (args.Length == 2 && args[1].Type == STJsonPathExpressionTokenType.String) {
                    var strText = args[1].Value.ToString();
                    arr_ch_trim = new char[strText.Length];
                    for (int i = 0; i < strText.Length; i++) {
                        arr_ch_trim[i] = strText[i];
                    }
                } else {
                    return STJsonPathExpressionToken.CreateError("Can not match the function: trim*(" + ME.GetFunctionArgTypeList(args) + "). ");
                }
                switch (nModel) {
                    case 0:
                        args[0].Value = args[0].Value.ToString().Trim(arr_ch_trim);
                        break;
                    case 1:
                        args[0].Value = args[0].Value.ToString().TrimStart(arr_ch_trim);
                        break;
                    case 2:
                        args[0].Value = args[0].Value.ToString().TrimEnd(arr_ch_trim);
                        break;
                }
                return args[0];
            } catch (Exception ex) {
                return STJsonPathExpressionToken.CreateError("Get an error when excute function: time*(" + ME.GetFunctionArgTypeList(args) + "). " + ex.Message);
            }
        }

        public static STJsonPathExpressionToken FN_time(STJsonPathExpressionToken[] args)
        {
            if (args == null || args.Length == 0) {
                return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.Long, (long)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)).TotalMilliseconds);
            }
            if (args.Length == 1) {
                if (args[0].Type == STJsonPathExpressionTokenType.String) {
                    try {
                        return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, DateTime.Now.ToString(args[0].Value.ToString()));
                    } catch (Exception ex) {
                        return STJsonPathExpressionToken.CreateError("Get an error when excute function: time(" + ME.GetFunctionArgTypeList(args) + "). " + ex.Message);
                    }
                }
            }
            if (args.Length == 2) {
                if (args[0].Type == STJsonPathExpressionTokenType.Long && args[1].Type == STJsonPathExpressionTokenType.String) {
                    var strTime =
                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                        .AddMilliseconds(Convert.ToInt64(args[0].Value))
                        .ToLocalTime()
                        .ToString(args[1].Value.ToString());
                    return STJsonPathExpressionToken.Create(-1, STJsonPathExpressionTokenType.String, strTime);
                }
            }
            return STJsonPathExpressionToken.CreateError("Can not match the function: time(" + ME.GetFunctionArgTypeList(args) + ")");
        }

        internal static string GetFunctionArgTypeList(STJsonPathExpressionToken[] args)
        {
            string[] arr_str = new string[args.Length];
            for (int i = 0; i < args.Length; i++) {
                arr_str[i] = args[i].Type.ToString();
            }
            return string.Join(",", arr_str);
        }

        //private static ExpressionToken FNGeneral(ExpressionToken[] args, GeneralCallback callback)
        //{
        //    if (args == null || args.Length < 1) {
        //        return ExpressionToken.Undefined;
        //    }
        //    var b_depth = args[0].Type == ExpressTokenType.Array;
        //    if (b_depth) {
        //        var arr = ME.GetArgs(args[0]);
        //        return ME.FNGeneralArray(arr, callback);
        //    }
        //    return callback(args[0]);
        //}

        ////private static ExpressionToken FNGeneral(ExpressionToken[] args, GeneralCallback callBack, bool bDepth)
        ////{
        ////    if (args == null || args.Length < 1) {
        ////        return ExpressionToken.Undefined;
        ////    }
        ////    if (args.Length > 1) {
        ////        bDepth = (bool)ME.FN_bool(args[1]).Value;
        ////    }
        ////    //bDepth |= args[0].Type == ExpressTokenType.Array;
        ////    if (bDepth) {
        ////        var arr = ME.GetArgs(args[0]);
        ////        return ME.FNGeneralArray(arr, callBack);
        ////    }
        ////    return callBack(args[0]);
        ////}

        //private static ExpressionToken FNGeneralArray(ExpressionToken[] tokens, GeneralCallback callBack)
        //{
        //    ExpressionToken[] results = new ExpressionToken[tokens.Length];
        //    for (int i = 0; i < tokens.Length; i++) {
        //        if (tokens[i].Type == ExpressTokenType.Array) {
        //            results[i] = ME.FNGeneralArray(tokens[i].Value as ExpressionToken[], callBack);
        //        } else {
        //            results[i] = callBack(tokens[i]);
        //        }
        //    }
        //    return ExpressionToken.Create(-1, ExpressTokenType.Array, results);
        //}

        //private static ExpressionToken[] GetArgs(ExpressionToken token)
        //{
        //    if (token.Type == ExpressTokenType.Array) {
        //        return token.Value as ExpressionToken[];
        //    }
        //    return new ExpressionToken[] { token };
        //}
    }
}
