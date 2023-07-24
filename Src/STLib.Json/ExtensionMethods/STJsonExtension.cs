using System;
using System.Collections.Generic;

using ME = STLib.Json.STJsonExtension;

namespace STLib.Json
{
    public static class STJsonExtension
    {
        private struct MergeSortRange
        {
            public int Left;
            public int Right;
        }

        private struct MergeSortInfo
        {
            public int Index;
            public double[] Values;

            //public override string ToString() {
            //    return "[" + Index + "] - {" + String.Join(",", Values) + "}";
            //}
        }

        public static string GetValue(this STJson json) {
            if (json.Value == null) {
                return null;
            }
            return json.Value.ToString();
        }

        public static string GetValue(this STJson json, string strJsonPath) {
            string strResult = null;
            json.GetValue(new STJsonPath(strJsonPath), null, out strResult);
            return strResult;
        }

        public static string GetValue(this STJson json, STJsonPath jsonPath) {
            string strResult = null;
            json.GetValue(jsonPath, null, out strResult);
            return strResult;
        }

        public static string GetValue(this STJson json, string strJsonPath, string defaltValue) {
            string strResult = null;
            json.GetValue(new STJsonPath(strJsonPath), defaltValue, out strResult);
            return strResult;
        }

        public static string GetValue(this STJson json, STJsonPath jsonPath, string defaltVale) {
            string strResult = null;
            json.GetValue(jsonPath, defaltVale, out strResult);
            return strResult;
        }

        public static bool GetValue(this STJson json, string strJsonPath, out string strResult) {
            return json.GetValue(new STJsonPath(strJsonPath), null, out strResult);
        }

        public static bool GetValue(this STJson json, STJsonPath jsonPath, out string strResult) {
            return json.GetValue(jsonPath, null, out strResult);
        }

        public static bool GetValue(this STJson json, string strJsonPath, string defaultValue, out string strResult) {
            return json.GetValue(new STJsonPath(strJsonPath), defaultValue, out strResult);
        }

        public static bool GetValue(this STJson json, STJsonPath jsonPath, string defaultValue, out string result) {
            result = defaultValue;
            var j = jsonPath.SelectFirst(json);
            if (j == null) {
                return false;
            }
            if (j.Value != null) {
                result = j.Value.ToString();
            } else {
                result = null;
            }
            return true;
        }

        // ==================================================================================================

        public static T GetValue<T>(this STJson json) {
            if (json.Value == null) {
                return default(T);
            }
            var t = typeof(T);
            bool bProcessed = true;
            var convert = STJsonBuildInConverter.Get(t);
            if (convert != null) {
                var value = convert.JsonToObject(t, json, ref bProcessed);
                if (bProcessed) {
                    return (T)value;
                }
            }
            return (T)json.Value;
        }

        public static T GetValue<T>(this STJson json, T defaultValue) {
            if (json.Value == null) {
                return defaultValue;
            }
            var t = typeof(T);
            try {
                bool bProcessed = true;
                var convert = STJsonBuildInConverter.Get(t);
                if (convert != null) {
                    var value = convert.JsonToObject(t, json, ref bProcessed);
                    if (bProcessed) {
                        return (T)value;
                    }
                }
                return (T)json.Value;
            } catch {
                return defaultValue;
            }
        }

        public static bool GetValue<T>(this STJson json, out T result) {
            if (json.Value == null) {
                result = default(T);
                return false;
            }
            try {
                var t = typeof(T);
                bool bProcessed = true;
                var convert = STJsonBuildInConverter.Get(t);
                if (convert != null) {
                    var value = convert.JsonToObject(t, json, ref bProcessed);
                    if (bProcessed) {
                        result = (T)value;
                        return true;
                    }
                }
                result = (T)json.Value;
                return true;
            } catch {
                result = default(T);
                return false;
            }
        }

        public static T GetValue<T>(this STJson json, string strJsonPath) {
            return json.GetValue<T>(new STJsonPath(strJsonPath));
        }

        public static T GetValue<T>(this STJson json, STJsonPath jsonPath) {
            var j = jsonPath.SelectFirst(json);
            if (j == null) {
                throw new STJsonPathException("Can not selected a object with path {" + jsonPath.SourceText + "}");
            }
            var t = typeof(T);
            bool bProcessed = true;
            var convert = STJsonBuildInConverter.Get(t);
            if (convert != null) {
                var value = convert.JsonToObject(t, j, ref bProcessed);
                if (bProcessed) {
                    return (T)value;
                }
            }
            return (T)j.Value;
        }

        public static T GetValue<T>(this STJson json, string strJsonPath, T defaultValue) {
            return json.GetValue<T>(new STJsonPath(strJsonPath), defaultValue);
        }

        public static T GetValue<T>(this STJson json, STJsonPath jsonPath, T defaultValue) {
            T reslt;
            json.GetValue<T>(jsonPath, defaultValue, out reslt);
            return reslt;
        }

        public static bool GetValue<T>(this STJson json, string strJsonPath, out T result) {
            return json.GetValue<T>(new STJsonPath(strJsonPath), default(T), out result);
        }

        public static bool GetValue<T>(this STJson json, STJsonPath jsonPath, out T result) {
            return json.GetValue<T>(jsonPath, default(T), out result);
        }

        public static bool GetValue<T>(this STJson json, string strJsonPath, T defaultValue, out T result) {
            return json.GetValue<T>(new STJsonPath(strJsonPath), defaultValue, out result);
        }

        public static bool GetValue<T>(this STJson json, STJsonPath jsonPath, T defaultValue, out T result) {
            result = defaultValue;
            var j = jsonPath.SelectFirst(json);
            if (j == null) {
                return false;
            }
            if (j.Value == null) {
                return false;
            }
            try {
                var t = typeof(T);
                bool bProcessed = true;
                var convert = STJsonBuildInConverter.Get(t);
                if (convert != null) {
                    var value = convert.JsonToObject(t, j, ref bProcessed);
                    if (bProcessed) {
                        result = (T)value;
                        return true;
                    }
                }
                result = (T)j.Value;
                return true;
            } catch {
                return false;
            }
        }

        // ==================================================================================================

        public static STJson Set(this STJson json, string strJsonPath, string value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, int value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, long value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, float value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, double value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, bool value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, DateTime value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, STJson value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, string strJsonPath, object value) {
            var jsonItemWithPath = new STJsonPath(strJsonPath).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }


        public static STJson Set(this STJson json, STJsonPath jsonPath, string value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, int value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, long value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, float value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, double value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, bool value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, DateTime value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, STJson value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(value);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, object value) {
            var jsonItemWithPath = jsonPath.CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath jsonPath, STJsonPathCallBack callBack) {
            var jsonItemWithPath = jsonPath.CreatePathJson(callBack);
            STJsonPath.RestorePathJson(json, jsonItemWithPath);
            return json;
        }

        // ==================================================================================================

        public static STJson Select(this STJson json, string strJsonPath) {
            return new STJsonPath(strJsonPath).Select(json, STJsonPathSelectMode.ItemOnly, null);
        }

        public static STJson Select(this STJson json, string strJsonPath, STJsonPathCallBack callBack) {
            return new STJsonPath(strJsonPath).Select(json, STJsonPathSelectMode.ItemOnly, callBack);
        }

        public static STJson Select(this STJson json, string strJsonPath, STJsonPathCallBackVoid callBack) {
            return new STJsonPath(strJsonPath).Select(json, STJsonPathSelectMode.ItemOnly, callBack);
        }

        public static STJson Select(this STJson json, string strJsonPath, STJsonPathSelectMode model) {
            return new STJsonPath(strJsonPath).Select(json, model, null);
        }

        public static STJson Select(this STJson json, string strJsonPath, STJsonPathSelectMode model, STJsonPathCallBack callBack) {
            return new STJsonPath(strJsonPath).Select(json, model, callBack);
        }

        public static STJson Select(this STJson json, string strJsonPath, STJsonPathSelectMode model, STJsonPathCallBackVoid callBack) {
            return new STJsonPath(strJsonPath).Select(json, model, callBack);
        }

        public static STJson Select(this STJson json, STJsonPath jsonPath) {
            return jsonPath.Select(json, STJsonPathSelectMode.ItemWithPath, null);
        }

        public static STJson Select(this STJson json, STJsonPath jsonPath, STJsonPathCallBack callBack) {
            return jsonPath.Select(json, STJsonPathSelectMode.ItemOnly, callBack);
        }

        public static STJson Select(this STJson json, STJsonPath jsonPath, STJsonPathCallBackVoid callBack) {
            return jsonPath.Select(json, STJsonPathSelectMode.ItemOnly, callBack);
        }

        public static STJson Select(this STJson json, STJsonPath jsonPath, STJsonPathSelectMode model) {
            return jsonPath.Select(json, model, null);
        }

        public static STJson Select(this STJson json, STJsonPath jsonPath, STJsonPathSelectMode model, STJsonPathCallBack callBack) {
            return jsonPath.Select(json, model, callBack);
        }

        public static STJson Select(this STJson json, STJsonPath jsonPath, STJsonPathSelectMode model, STJsonPathCallBackVoid callBack) {
            return jsonPath.Select(json, model, callBack);
        }


        public static STJson SelectFirst(this STJson json, string strJsonPath) {
            return new STJsonPath(strJsonPath).SelectFirst(json);
        }

        public static STJson SelectFirst(this STJson json, string strJsonPath, STJsonPathSelectMode model) {
            return new STJsonPath(strJsonPath).SelectFirst(json, model);
        }

        public static STJson SelectLast(this STJson json, string strJsonPath) {
            return new STJsonPath(strJsonPath).SelectLast(json);
        }

        public static STJson SelectLast(this STJson json, string strJsonPath, STJsonPathSelectMode model) {
            return new STJsonPath(strJsonPath).SelectLast(json, model);
        }

        public static STJson SelectFirst(this STJson json, STJsonPath jsonPath) {
            return jsonPath.SelectFirst(json);
        }

        public static STJson SelectFirst(this STJson json, STJsonPath jsonPath, STJsonPathSelectMode model) {
            return jsonPath.SelectFirst(json, model);
        }

        public static STJson SelectLast(this STJson json, STJsonPath jsonPath) {
            return jsonPath.SelectLast(json);
        }

        public static STJson SelectLast(this STJson json, STJsonPath jsonPath, STJsonPathSelectMode model) {
            return jsonPath.SelectLast(json, model);
        }

        // ==================================================================================================

        public static STJson Group(this STJson json, params string[] strPaths) {
            return json.Group(ME.StrToPath(strPaths));
        }

        public static STJson Group(this STJson json, params STJsonPath[] paths) {
            ME.CheckPathsName(paths);
            var json_ret = new STJson();
            var lst_dic = new List<Dictionary<object, List<STJson>>>();
            foreach (var v in paths) {
                lst_dic.Add(new Dictionary<object, List<STJson>>());
            }
            object obj_temp = null;
            foreach (var v in json) {
                if (v.ValueType != STJsonValueType.Object) {
                    continue;
                }
                for (int i = 0; i < paths.Length; i++) {
                    var dic = lst_dic[i];
                    var json_temp = paths[i].SelectFirst(v);
                    if (json_temp == null) {
                        continue;
                    }
                    switch (json_temp.ValueType) {
                        case STJsonValueType.String:
                        case STJsonValueType.Long:
                        case STJsonValueType.Double:
                        case STJsonValueType.Boolean:
                        case STJsonValueType.Datetime:
                            obj_temp = json_temp.Value;
                            break;

                    }
                    if (obj_temp == null) {
                        continue;
                    }
                    if (!dic.ContainsKey(obj_temp)) {
                        dic.Add(obj_temp, new List<STJson>());
                    }
                    dic[obj_temp].Add(v);
                }
            }
            for (int i = 0; i < lst_dic.Count; i++) {
                var json_group = STJson.CreateArray();
                foreach (var v in lst_dic[i]) {
                    json_group.Append(new STJson()
                        .SetItem("value", v.Key)
                        .SetItem("items", v.Value)
                        );
                }
                json_ret.SetItem(paths[i].Name, json_group);
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Terms(this STJson json) {
            List<object> lst = new List<object>();
            foreach (var v in json) {
                ME.GetTermsValue(v, lst);
            }
            Dictionary<object, int> dic = new Dictionary<object, int>();
            foreach (var v in lst) {
                if (!dic.ContainsKey(v)) {
                    dic.Add(v, 1);
                } else {
                    dic[v]++;
                }
            }
            STJson json_ret = STJson.CreateArray();
            foreach (var v in dic) {
                json_ret.Append(new STJson().SetItem("value", v.Key).SetItem("count", v.Value));
            }
            return json_ret;
        }

        public static STJson Terms(this STJson json, params string[] strPaths) {
            return json.Terms(ME.StrToPath(strPaths));
        }

        public static STJson Terms(this STJson json, params STJsonPath[] paths) {
            ME.CheckPathsName(paths);
            STJson json_ret = new STJson();
            List<object>[] lsts = new List<object>[paths.Length];
            for (int i = 0; i < paths.Length; i++) {
                lsts[i] = new List<object>();
            }
            foreach (var v in json) {
                for (int i = 0; i < paths.Length; i++) {
                    var temp = paths[i].SelectFirst(v);
                    if (temp == null) {
                        continue;
                    }
                    ME.GetTermsValue(temp, lsts[i]);
                }
            }
            Dictionary<object, int> dic = new Dictionary<object, int>();
            for (int i = 0; i < paths.Length; i++) {
                dic.Clear();
                foreach (var v in lsts[i]) {
                    if (!dic.ContainsKey(v)) {
                        dic.Add(v, 1);
                    } else {
                        dic[v]++;
                    }
                }
                var json_terms = STJson.CreateArray();
                foreach (var v in dic) {
                    json_terms.Append(new STJson().SetItem("value", v.Key).SetItem("count", v.Value));
                }
                json_ret.SetItem(paths[i].Name, json_terms);
            }
            return json_ret;
        }

        private static void GetTermsValue(STJson json, List<object> lstResult) {
            switch (json.ValueType) {
                case STJsonValueType.Boolean:
                case STJsonValueType.String:
                case STJsonValueType.Long:
                case STJsonValueType.Double:
                case STJsonValueType.Datetime:
                    lstResult.Add(json.Value);
                    return;
                case STJsonValueType.Array:
                    foreach (var v in json) {
                        ME.GetTermsValue(v, lstResult);
                    }
                    return;
            }
        }

        // ==================================================================================================

        public static STJson Sort(this STJson json) {
            return json.Sort(false);
        }

        public static STJson Sort(this STJson json, bool isDesc) {
            return json.Sort(STJson.FromObject(new object[]{new {
                path = "",
                desc = isDesc
            } }));
        }

        public static STJson Sort(this STJson json, params object[] fields) {
            var jsonSort = STJson.CreateArray();
            for (int i = 0; i < fields.Length; i += 2) {
                var j = new STJson()
                    .SetItem("path", fields[i] == null ? null : fields[i].ToString())
                    .SetItem("desc", i + 1 < fields.Length ? Convert.ToBoolean(fields[i + 1]) : false);
                jsonSort.Append(j);
            }
            return json.Sort(jsonSort);
        }

        private static STJson Sort(this STJson json, STJson jsonSort) {
            if (json.ValueType != STJsonValueType.Array) {
                throw new Exception();
            }
            var msis = ME.GetMergeSortInfos(json, jsonSort);
            int nKeysLen = jsonSort.Count;
            var msis_temp = new MergeSortInfo[msis.Length];
            ME.MergeSort(msis_temp, msis, jsonSort.Count, 0, msis.Length - 1);
            List<STJson> lst = new List<STJson>(msis.Length);
            for (int i = 0; i < msis.Length; i++) {
                lst.Add(json[msis[i].Index]);
            }
            STJson json_ret = new STJson();
            json_ret.SetArray(lst);
            return json_ret;
        }

        private static MergeSortInfo[] GetMergeSortInfos(STJson json, STJson jsonSort) {
            double d_temp = 0;
            var arr_path = new STJsonPath[jsonSort.Count];
            bool[] arr_b_desc = new bool[jsonSort.Count];
            for (int i = 0; i < jsonSort.Count; i++) {
                var str = jsonSort[i]["path"].GetValue();
                if (!string.IsNullOrEmpty(str)) {
                    arr_path[i] = new STJsonPath(str, str);
                }
                arr_b_desc[i] = jsonSort[i]["desc"].GetValue<bool>();
            }
            MergeSortInfo[] msis = new MergeSortInfo[json.Count];
            for (int i = 0; i < json.Count; i++) {
                var item = json[i];
                msis[i].Index = i;
                msis[i].Values = new double[arr_path.Length];
                for (int j = 0; j < arr_path.Length; j++) {
                    if (arr_path[j] != null) {
                        item = arr_path[j].SelectFirst(json[i]);
                    }
                    if (item == null || item.IsNullObject) {
                        d_temp = arr_b_desc[j] ? double.MinValue : double.MaxValue;
                    } else {
                        switch (item.ValueType) {
                            case STJsonValueType.Long:
                            case STJsonValueType.Double:
                                d_temp = item.GetValue<double>();
                                break;
                            case STJsonValueType.Boolean:
                                d_temp = item.GetValue<bool>() ? 1 : 0;
                                break;
                            case STJsonValueType.Datetime:
                                d_temp = Convert.ToDouble(item.GetValue<DateTime>());
                                break;
                            case STJsonValueType.String:
                                d_temp = item.Value == null ? 0 : item.Value.ToString().Length;
                                break;
                            case STJsonValueType.Array:
                                d_temp = item.Count;
                                break;
                            default:
                                d_temp = arr_b_desc[j] ? double.MinValue : double.MaxValue;
                                break;
                        }
                    }
                    msis[i].Values[j] = arr_b_desc[j] ? -d_temp : d_temp;
                }
            }
            return msis;
        }

        private static void MergeSort(MergeSortInfo[] msis_temp, MergeSortInfo[] msis_src, int nKeysLen, int nLeft, int nRight) {
            int nMid = (nLeft + nRight) / 2;
            if (nLeft >= nRight) {
                return;
            }
            var rangeL = new MergeSortRange() { Left = nLeft, Right = nMid };
            var rangeR = new MergeSortRange() { Left = nMid + 1, Right = nRight };
            ME.MergeSort(msis_temp, msis_src, nKeysLen, nLeft, nMid);
            ME.MergeSort(msis_temp, msis_src, nKeysLen, nMid + 1, nRight);
            ME.MergeSortMerge(msis_temp, msis_src, nKeysLen, rangeL, rangeR);
        }

        private static void MergeSortMerge(MergeSortInfo[] msis_temp, MergeSortInfo[] msis_src, int nKeysLen, MergeSortRange rangeL, MergeSortRange rangeR) {
            int nIndex = 0;
            int nLeft = rangeL.Left;
            int nRight = rangeR.Left;
            double d_l = 0, d_r = 0;
            while (nLeft <= rangeL.Right && nRight <= rangeR.Right) {
                d_l = d_r = 0;
                for (int i = 0; i < nKeysLen; i++) {
                    if (msis_src[nLeft].Values[i] != msis_src[nRight].Values[i]) {
                        d_l = msis_src[nLeft].Values[i];
                        d_r = msis_src[nRight].Values[i];
                        break;
                    }
                }
                if (d_l < d_r) {
                    msis_temp[nIndex++] = msis_src[nLeft++];
                } else {
                    msis_temp[nIndex++] = msis_src[nRight++];
                }
            }
            while (nLeft <= rangeL.Right) {
                msis_temp[nIndex++] = msis_src[nLeft++];
            }
            while (nRight <= rangeR.Right) {
                msis_temp[nIndex++] = msis_src[nRight++];
            }
            for (int i = rangeL.Left, j = 0; i <= rangeR.Right; i++, j++) {
                msis_src[i] = msis_temp[j];
            }
        }

        // ==================================================================================================

        public static STJson Min(this STJson json) {
            int nCounter = 0;
            double d_min = double.MaxValue, d_val = 0;
            STJson json_min = STJson.CreateArray();
            foreach (var v in json) {
                if (v.ValueType != STJsonValueType.Long) {
                    continue;
                }
                if (v.ValueType != STJsonValueType.Double) {
                    continue;
                }
                nCounter++;
                d_val = v.GetValue<double>();
                if (d_val < d_min) {
                    d_min = d_val;
                    json_min.Clear();
                    json_min.Append(v);
                } else if (d_val == d_min) {
                    json_min.Append(v);
                }
            }
            return new STJson()
                .SetItem("count", nCounter)
                .SetItem("value", nCounter == 0 ? 0 : d_min)
                .SetItem("items", json_min);
        }

        public static STJson Min(this STJson json, params string[] strPaths) {
            return json.Min(ME.StrToPath(strPaths));
        }

        public static STJson Min(this STJson json, params STJsonPath[] paths) {
            ME.CheckPathsName(paths);
            List<int> lst_counter = new List<int>();
            List<double> lst_min_val = new List<double>();
            List<STJson> lst_min_json = new List<STJson>();
            foreach (var v in paths) {
                lst_counter.Add(0);
                lst_min_val.Add(double.MaxValue);
                lst_min_json.Add(STJson.CreateArray());
            }
            double d_val = 0;
            foreach (var v in json) {
                for (int i = 0; i < paths.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = paths[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Double) {
                        continue;
                    }
                    lst_counter[i]++;
                    d_val = json_item.GetValue<double>();
                    if (lst_min_val[i] > d_val) {
                        lst_min_val[i] = d_val;
                        lst_min_json[i].Clear();
                        lst_min_json[i].Append(v);
                    } else if (lst_min_val[i] == d_val) {
                        lst_min_json[i].Append(v);
                    }
                }
            }
            STJson json_ret = new STJson();
            for (int i = 0; i < paths.Length; i++) {
                json_ret.SetItem(paths[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_min_val[i])
                    .SetItem("items", lst_min_json[i])
                    );
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Max(this STJson json) {
            int nCounter = 0;
            double d_min = double.MinValue, d_val = 0;
            STJson json_max = STJson.CreateArray();
            foreach (var v in json) {
                if (v.ValueType != STJsonValueType.Long) {
                    continue;
                }
                if (v.ValueType != STJsonValueType.Double) {
                    continue;
                }
                nCounter++;
                d_val = v.GetValue<double>();
                if (d_val > d_min) {
                    d_min = d_val;
                    json_max.Clear();
                    json_max.Append(v);
                } else if (d_val == d_min) {
                    json_max.Append(v);
                }
            }
            return new STJson()
                .SetItem("count", nCounter)
                .SetItem("value", nCounter == 0 ? 0 : d_min)
                .SetItem("items", json_max);
        }

        public static STJson Max(this STJson json, params string[] strPaths) {
            return json.Max(ME.StrToPath(strPaths));
        }

        public static STJson Max(this STJson json, params STJsonPath[] paths) {
            ME.CheckPathsName(paths);
            List<int> lst_counter = new List<int>();
            List<double> lst_max_val = new List<double>();
            List<STJson> lst_max_json = new List<STJson>();
            foreach (var v in paths) {
                lst_counter.Add(0);
                lst_max_val.Add(double.MinValue);
                lst_max_json.Add(STJson.CreateArray());
            }
            double d_val = 0;
            foreach (var v in json) {
                for (int i = 0; i < paths.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = paths[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Double) {
                        continue;
                    }
                    lst_counter[i]++;
                    d_val = json_item.GetValue<double>();
                    if (lst_max_val[i] < d_val) {
                        lst_max_val[i] = d_val;
                        lst_max_json[i].Clear();
                        lst_max_json[i].Append(v);
                    } else if (lst_max_val[i] == d_val) {
                        lst_max_json[i].Append(v);
                    }
                }
            }
            STJson json_ret = new STJson();
            for (int i = 0; i < paths.Length; i++) {
                json_ret.SetItem(paths[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_max_val[i])
                    .SetItem("items", lst_max_json[i])
                    );
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Sum(this STJson json) {
            int nCounter = 0;
            double d_sum = 0;
            foreach (var v in json) {
                if (v.ValueType != STJsonValueType.Long) {
                    continue;
                }
                if (v.ValueType != STJsonValueType.Double) {
                    continue;
                }
                nCounter++;
                d_sum += v.GetValue<double>();
            }
            return new STJson().SetItem("count", nCounter).SetItem("value", nCounter == 0 ? 0 : d_sum);
        }

        public static STJson Sum(this STJson json, params string[] strPaths) {
            return json.Sum(ME.StrToPath(strPaths));
        }

        public static STJson Sum(this STJson json, params STJsonPath[] paths) {
            ME.CheckPathsName(paths);
            List<int> lst_counter = new List<int>();
            List<double> lst_sum = new List<double>();
            foreach (var v in paths) {
                lst_counter.Add(0);
                lst_sum.Add(0);
            }
            foreach (var v in json) {
                for (int i = 0; i < paths.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = paths[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Double) {
                        continue;
                    }
                    lst_counter[i]++;
                    lst_sum[i] += json_item.GetValue<double>();
                }
            }
            STJson json_ret = new STJson();
            for (int i = 0; i < paths.Length; i++) {
                json_ret.SetItem(paths[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_sum[i])
                    );
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Avg(this STJson json) {
            int nCounter = 0;
            double d_sum = 0;
            foreach (var v in json) {
                if (v.ValueType != STJsonValueType.Long) {
                    continue;
                }
                if (v.ValueType != STJsonValueType.Double) {
                    continue;
                }
                nCounter++;
                d_sum += v.GetValue<double>();
            }
            return new STJson().SetItem("count", nCounter).SetItem("value", nCounter == 0 ? 0 : d_sum / nCounter);
        }

        public static STJson Avg(this STJson json, params string[] strPaths) {
            return json.Avg(ME.StrToPath(strPaths));
        }

        public static STJson Avg(this STJson json, params STJsonPath[] paths) {
            ME.CheckPathsName(paths);
            List<int> lst_counter = new List<int>();
            List<double> lst_sum = new List<double>();
            foreach (var v in paths) {
                lst_counter.Add(0);
                lst_sum.Add(0);
            }
            foreach (var v in json) {
                for (int i = 0; i < paths.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = paths[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Double) {
                        continue;
                    }
                    lst_counter[i]++;
                    lst_sum[i] += json_item.GetValue<double>();
                }
            }
            STJson json_ret = new STJson();
            for (int i = 0; i < paths.Length; i++) {
                json_ret.SetItem(paths[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_sum[i] / lst_counter[i])
                    );
            }
            return json_ret;
        }

        private static STJsonPath[] StrToPath(string[] strPaths) {
            var paths = new STJsonPath[strPaths.Length];
            for (int i = 0; i < strPaths.Length; i++) {
                paths[i] = new STJsonPath(strPaths[i], strPaths[i]);
            }
            return paths;
        }

        private static void CheckPathsName(STJsonPath[] paths) {
            var hs_name = new HashSet<string>();
            foreach (var v in paths) {
                if (!hs_name.Add(v.Name)) {
                    throw new STJsonAggregateException("The same path name {" + v.Name + "}");
                }
            }
        }
    }
}
