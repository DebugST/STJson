using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ME = STLib.Json.STJsonPathBuildInFunctions;

namespace STLib.Json
{
    internal class STJsonPathBuildInFunctions
    {
        private delegate ExpressionToken GeneralCallBack(ExpressionToken token);

        private static List<FNInfo> m_lst_info;

        public static STJson FuncList { get; private set; }

        [STJson(STJsonSerilizaMode.Exclude)]
        private struct FNInfo
        {
            public string name;
            public string[] demos;
            [STJsonProperty]
            public STJsonPathBuildInFuncHander func;

            public FNInfo(string strName, STJsonPathBuildInFuncHander fn, string[] demos) {
                this.name = strName;
                this.func = fn;
                this.demos = demos;
            }
        }

        public static void Init() {
            m_lst_info = new List<FNInfo>();
            m_lst_info.Add(new FNInfo("typeof", ME.FN_typeof, "(object) -> typeof('abc')|(array,bool) -> typeof(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("str", ME.FN_str, "(object) -> str('abc')|(array,bool) -> str(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("upper", ME.FN_upper, "(object) -> upper('abc')|(array,bool) -> upper(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("lower", ME.FN_lower, "(object) -> lower('abc')|(array,bool) -> lower(['abc',123],true)".Split('|')));

            m_lst_info.Add(new FNInfo("len", ME.FN_len, "(object) -> len('abc')|(array,bool) -> len(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("long", ME.FN_long, "(object) -> long('abc')|(array,bool) -> long(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("double", ME.FN_double, "(object) -> double('abc')|(array,bool) -> double(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("bool", ME.FN_bool, "(object) -> bool('abc')|(array,bool) -> bool(['abc',123],true)".Split('|')));

            m_lst_info.Add(new FNInfo("abs", ME.FN_abs, "(object) -> abs('abc')|(array,bool) -> abs(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("round", ME.FN_round, "(object) -> round('abc')|(array,bool) -> round(['abc',123],true)".Split('|')));
            m_lst_info.Add(new FNInfo("ceil", ME.FN_lower, "(object) -> ceil('abc')|(array,bool) -> ceil(['abc',123],true)".Split('|')));

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

        public static ExpressionToken FN_typeof(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_typeof, false);
        }

        private static ExpressionToken FN_typeof(ExpressionToken token) {
            switch (token.Type) {
                case ExpressTokenType.String: return ExpressionToken.Create(-1, ExpressTokenType.String, "string");
                case ExpressTokenType.Long: return ExpressionToken.Create(-1, ExpressTokenType.String, "long");
                case ExpressTokenType.Double: return ExpressionToken.Create(-1, ExpressTokenType.String, "double");
                case ExpressTokenType.Boolean: return ExpressionToken.Create(-1, ExpressTokenType.String, "boolean");
                case ExpressTokenType.Array: return ExpressionToken.Create(-1, ExpressTokenType.String, "array");
                case ExpressTokenType.Object: return ExpressionToken.Create(-1, ExpressTokenType.String, "object");
            }
            return ExpressionToken.Undefined;
        }

        public static ExpressionToken FN_str(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_str, false);
        }

        private static ExpressionToken FN_str(ExpressionToken token) {
            return ExpressionToken.Create(-1, ExpressTokenType.String, Convert.ToString(token.Value));
        }

        public static ExpressionToken FN_upper(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_upper, true);
        }

        private static ExpressionToken FN_upper(ExpressionToken token) {
            switch (token.Type) {
                case ExpressTokenType.String:
                    return ExpressionToken.Create(-1, ExpressTokenType.String, Convert.ToString(token.Value).ToUpper());
            }
            return token;
        }

        public static ExpressionToken FN_lower(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_lower, true);
        }

        private static ExpressionToken FN_lower(ExpressionToken token) {
            switch (token.Type) {
                case ExpressTokenType.String:
                    return ExpressionToken.Create(-1, ExpressTokenType.String, Convert.ToString(token.Value).ToLower());
            }
            return token;
        }

        public static ExpressionToken FN_len(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_len, false);
        }

        private static ExpressionToken FN_len(ExpressionToken token) {
            switch (token.Type) {
                case ExpressTokenType.String:
                    return ExpressionToken.Create(-1, ExpressTokenType.Long, Convert.ToString(token.Value).Length);
                case ExpressTokenType.Array:
                    return ExpressionToken.Create(-1, ExpressTokenType.Long, (token.Value as ExpressionToken[]).Length);
            }
            return ExpressionToken.Undefined;
        }

        public static ExpressionToken FN_long(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_long, true);
        }

        private static ExpressionToken FN_long(ExpressionToken token) {
            try {
                return ExpressionToken.Create(-1, ExpressTokenType.Long, Convert.ToInt64(token.Value));
            } catch {
                return ExpressionToken.Undefined;
            }
        }

        public static ExpressionToken FN_double(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_double, true);
        }

        private static ExpressionToken FN_double(ExpressionToken token) {
            try {
                return ExpressionToken.Create(-1, ExpressTokenType.Long, Convert.ToDouble(token.Value));
            } catch {
                return ExpressionToken.Undefined;
            }
        }

        public static ExpressionToken FN_bool(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_bool, true);
        }

        private static ExpressionToken FN_bool(ExpressionToken token) {
            try {
                return ExpressionToken.Create(-1, ExpressTokenType.Boolean, Convert.ToBoolean(token.Value));
            } catch {
                return ExpressionToken.Create(-1, ExpressTokenType.Boolean, false);
            }
        }

        public static ExpressionToken FN_abs(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_abs, true);
        }

        private static ExpressionToken FN_abs(ExpressionToken token) {
            long l_v = 0;
            double d_v = 0;
            switch (token.Type) {
                case ExpressTokenType.Long:
                    l_v = Convert.ToInt64(token.Value);
                    if (l_v < 0) {
                        token.Value = -l_v;
                    }
                    break;
                case ExpressTokenType.Double:
                    d_v = Convert.ToDouble(token.Value);
                    if (d_v < 0) {
                        token.Value = -d_v;
                    }
                    break;
            }
            return token;
        }

        public static ExpressionToken FN_round(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_round, true);
        }

        private static ExpressionToken FN_round(ExpressionToken token) {
            switch (token.Type) {
                case ExpressTokenType.Double:
                    token.Type = ExpressTokenType.Long;
                    token.Value = (long)Math.Round(Convert.ToDouble(token.Value));
                    break;
            }
            return token;
        }

        public static ExpressionToken FN_ceil(ExpressionToken[] args) {
            return ME.FNGeneral(args, ME.FN_ceil, true);
        }

        private static ExpressionToken FN_ceil(ExpressionToken token) {
            switch (token.Type) {
                case ExpressTokenType.Double:
                    token.Type = ExpressTokenType.Long;
                    token.Value = (long)Math.Ceiling(Convert.ToDouble(token.Value));
                    break;
            }
            return token;
        }


        private static ExpressionToken FN_max(ExpressionToken[] args) {
            if (args == null || args.Length != 1) {
                return ExpressionToken.Undefined;
            }
            if (args[0].Type != ExpressTokenType.Array) {
                return ExpressionToken.Undefined;
            }
            ExpressionToken token = ExpressionToken.Create(-1, ExpressTokenType.Double, double.MinValue);
            double d_v = double.MinValue, d_t = 0;
            foreach (var v in args[0].Value as ExpressionToken[]) {
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

        private static ExpressionToken FN_min(ExpressionToken[] args) {
            if (args == null || args.Length != 1) {
                return ExpressionToken.Undefined;
            }
            if (args[0].Type != ExpressTokenType.Array) {
                return ExpressionToken.Undefined;
            }
            ExpressionToken token = ExpressionToken.Create(-1, ExpressTokenType.Double, double.MaxValue);
            double d_v = double.MaxValue, d_t = 0;
            foreach (var v in args[0].Value as ExpressionToken[]) {
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

        private static ExpressionToken FN_avg(ExpressionToken[] args) {
            if (args == null || args.Length != 1) {
                return ExpressionToken.Undefined;
            }
            if (args[0].Type != ExpressTokenType.Array) {
                return ExpressionToken.Undefined;
            }
            double d_v = 0;
            int nCounter = 0;
            foreach (var v in args[0].Value as ExpressionToken[]) {
                if (!v.IsNumber) {
                    continue;
                }
                nCounter++;
                d_v += Convert.ToDouble(v.Value);
            }
            if (nCounter == 0) {
                return ExpressionToken.Create(-1, ExpressTokenType.Double, 0);
            }
            return ExpressionToken.Create(-1, ExpressTokenType.Double, d_v / nCounter);
        }

        private static ExpressionToken FN_sum(ExpressionToken[] args) {
            if (args == null || args.Length != 1) {
                return ExpressionToken.Undefined;
            }
            if (args[0].Type != ExpressTokenType.Array) {
                return ExpressionToken.Undefined;
            }
            double d_v = 0;
            foreach (var v in args[0].Value as ExpressionToken[]) {
                if (!v.IsNumber) {
                    continue;
                }
                d_v += Convert.ToDouble(v.Value);
            }
            return ExpressionToken.Create(-1, ExpressTokenType.Double, d_v);
        }


        public static ExpressionToken FN_split(ExpressionToken[] args) {
            if (args == null || args.Length != 2) {
                return ExpressionToken.Undefined;
            }
            if (args[0].Type != ExpressTokenType.String || args[1].Type != ExpressTokenType.String) {
                return ExpressionToken.Undefined;
            }
            try {
                Regex reg = new Regex(args[1].Value.ToString());
                var strs = reg.Split(args[0].Value.ToString());
                var results = new ExpressionToken[strs.Length];
                for (int i = 0; i < strs.Length; i++) {
                    results[i] = ExpressionToken.Create(-1, ExpressTokenType.String, strs[i]);
                }
                return ExpressionToken.Create(-1, ExpressTokenType.Array, results);
            } catch {
                return ExpressionToken.Undefined;
            }
        }

        public static ExpressionToken FN_trim(ExpressionToken[] args) {
            return ME.FN_trim(args, 0);
        }

        private static ExpressionToken FN_trims(ExpressionToken[] args) {
            return ME.FN_trim(args, 1);
        }

        private static ExpressionToken FN_trime(ExpressionToken[] args) {
            return ME.FN_trim(args, 2);
        }

        private static ExpressionToken FN_trim(ExpressionToken[] args, int nModel) {
            if (args == null || args.Length < 1) {
                return ExpressionToken.Undefined;
            }
            if (args[0].Type != ExpressTokenType.String) {
                return ExpressionToken.Undefined;
            }
            char[] chTrim = null;
            try {
                if (args.Length > 1 && args[1].Type == ExpressTokenType.String) {
                    var strText = args[1].Value.ToString();
                    chTrim = new char[strText.Length];
                    for (int i = 0; i < strText.Length; i++) {
                        chTrim[i] = strText[i];
                    }
                } else {
                    chTrim = new char[] { ' ', '\t', '\r', '\n' };
                }
                switch (nModel) {
                    case 0:
                        args[0].Value = args[0].Value.ToString().Trim(chTrim);
                        break;
                    case 1:
                        args[0].Value = args[0].Value.ToString().TrimStart(chTrim);
                        break;
                    case 2:
                        args[0].Value = args[0].Value.ToString().TrimEnd(chTrim);
                        break;
                }
                return args[0];
            } catch {
                return ExpressionToken.Undefined;
            }
        }

        public static ExpressionToken FN_time(ExpressionToken[] args) {
            if (args == null || args.Length == 0) {
                return ExpressionToken.Create(-1, ExpressTokenType.Long, (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
            }
            if (args.Length == 1) {
                if (args[0].Type == ExpressTokenType.String) {
                    return ExpressionToken.Create(-1, ExpressTokenType.String, DateTime.Now.ToString(args[0].Value.ToString()));
                }
            }
            if (args.Length == 2) {
                if (args[0].Type == ExpressTokenType.Long && args[1].Type == ExpressTokenType.String) {
                    var strTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))
                        .AddMilliseconds(Convert.ToInt64(args[0].Value))
                        .ToString(args[1].Value.ToString());
                    return ExpressionToken.Create(-1, ExpressTokenType.String, strTime);
                }
            }
            return ExpressionToken.Undefined;
        }


        private static ExpressionToken FNGeneral(ExpressionToken[] args, GeneralCallBack callBack, bool bDepth) {
            if (args == null || args.Length < 1) {
                return ExpressionToken.Undefined;
            }
            if (args.Length > 1) {
                bDepth = (bool)ME.FN_bool(args[1]).Value;
            }
            //bDepth |= args[0].Type == ExpressTokenType.Array;
            if (bDepth) {
                var arr = ME.GetArgs(args[0]);
                return ME.FNGeneral(arr, callBack);
            }
            return callBack(args[0]);
        }

        private static ExpressionToken FNGeneral(ExpressionToken[] tokens, GeneralCallBack callBack) {
            ExpressionToken[] results = new ExpressionToken[tokens.Length];
            for (int i = 0; i < tokens.Length; i++) {
                if (tokens[i].Type == ExpressTokenType.Array) {
                    results[i] = ME.FNGeneral(tokens[i].Value as ExpressionToken[], callBack);
                } else {
                    results[i] = callBack(tokens[i]);
                }
            }
            return ExpressionToken.Create(-1, ExpressTokenType.Array, results);
        }

        private static ExpressionToken[] GetArgs(ExpressionToken token) {
            if (token.Type == ExpressTokenType.Array) {
                return token.Value as ExpressionToken[];
            }
            return new ExpressionToken[] { token };
        }
    }
}
