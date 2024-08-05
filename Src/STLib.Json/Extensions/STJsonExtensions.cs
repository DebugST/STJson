using System;
using System.Collections.Generic;

using ME = STLib.Json.STJsonExtensions;

namespace STLib.Json
{
    public static class STJsonExtensions
    {
        public delegate int STJsonSortCallback(STJson json_a, STJson json_b);
        public delegate void STJsonForEachCallback(STJson json);

        #region Bind Object

        //private static Type m_type_string = typeof(string);

        //public static STJson ToSTJson(this object obj)
        //{
        //    var type = obj.GetType();
        //    if (type == m_type_string) {
        //        return STJson.Deserialize(obj.ToString());
        //    }
        //    return STJson.FromObject(obj);
        //}

        //public static string ToSTJsonString(this object obj)
        //{
        //    return STJson.Serialize(obj, STJsonSetting.Default);
        //}

        //public static string ToSTJsonString(this object obj, int n_space_count)
        //{
        //    var type = obj.GetType();
        //    if (type == m_type_string) {
        //        return STJson.Deserialize(obj.ToString()).ToString(n_space_count);
        //    }
        //    return STJson.Serialize(obj, n_space_count);
        //}

        //public static string ToSTJsonString(this object obj, int n_space_count, STJsonSetting setting)
        //{
        //    return STJson.Serialize(obj, n_space_count, setting);
        //}

        #endregion Bind 

        public static bool IsNullOrNullValue(this STJson json)
        {
            return json == null || json.IsNullValue;
        }

        public static bool IsNullOrNullValue(this STJson json, string str_path)
        {
            return json.IsNullOrNullValue(new STJsonPath(str_path));
        }

        public static bool IsNullOrNullValue(this STJson json, STJsonPath json_path)
        {
            if (json == null || json.IsNullValue) {
                return true;
            }
            var j = json_path.SelectFirst(json);
            return j == null || j.IsNullValue;
        }

        public static STJson ForEach(this STJson json, STJsonForEachCallback callback)
        {
            if (json == null) return json;
            foreach (var j in json) {
                callback(j);
            }
            return json;
        }

        public static string GetValue(this STJson json, string str_path)
        {
            var json_path = new STJsonPath(str_path);
            var j = json.SelectFirst(json_path);
            return j.GetValue();
        }

        public static string GetValue(this STJson json, STJsonPath json_path)
        {
            var j = json.SelectFirst(json_path);
            return j.GetValue();
        }

        // ==================================================================================================

        public static T GetValue<T>(this STJson json, string str_path)
        {
            return json.GetValue<T>(new STJsonPath(str_path));
        }

        public static T GetValue<T>(this STJson json, STJsonPath json_path)
        {
            var j = json_path.SelectFirst(json);
            if (j == null) {
                throw new STJsonPathException("Can not selected a object with path {" + json_path.SourceText + "}");
            }
            return json.GetValue<T>();
            /*
            var t = typeof(T);
            bool b_processed = true;
            var convert = STJsonBuildInConverter.Get(t);
            if (convert != null) {
                var value = convert.JsonToObject(t, j, ref b_processed);
                if (b_processed) {
                    return (T)value;
                }
            }
            return STJson.Deserialize<T>(j);*/
        }

        public static T GetValue<T>(this STJson json, string str_path, T default_value)
        {
            return json.GetValue<T>(new STJsonPath(str_path), default_value);
        }

        public static T GetValue<T>(this STJson json, STJsonPath json_path, T default_value)
        {
            T result;
            json.GetValue<T>(json_path, default_value, out result);
            return result;
        }

        public static bool GetValue<T>(this STJson json, string str_path, out T result)
        {
            return json.GetValue<T>(new STJsonPath(str_path), default(T), out result);
        }

        public static bool GetValue<T>(this STJson json, STJsonPath json_path, out T result)
        {
            return json.GetValue<T>(json_path, default(T), out result);
        }

        public static bool GetValue<T>(this STJson json, string str_path, T default_value, out T result)
        {
            return json.GetValue<T>(new STJsonPath(str_path), default_value, out result);
        }

        public static bool GetValue<T>(this STJson json, STJsonPath json_path, T default_value, out T result)
        {
            result = default_value;
            if (json == null) return false;
            var j = json_path.SelectFirst(json);
            if (j == null || j.Value == null) {
                return false;
            }
            if (j.GetValue(out result)) {
                return true;
            }
            result = default_value;
            return false;
        }

        // ==================================================================================================

        public static STJson Set(this STJson json, string str_path, object value)
        {
            var json_item_with_path = new STJsonPath(str_path).CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, json_item_with_path);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath json_path, object value)
        {
            var json_item_with_path = json_path.CreatePathJson(STJson.FromObject(value));
            STJsonPath.RestorePathJson(json, json_item_with_path);
            return json;
        }

        public static STJson Set(this STJson json, STJsonPath json_path, STJsonPathCallBack callback)
        {
            var json_item_with_path = json_path.CreatePathJson(callback);
            STJsonPath.RestorePathJson(json, json_item_with_path);
            return json;
        }

        // ==================================================================================================

        public static STJson Select(this STJson json, string str_path)
        {
            return new STJsonPath(str_path).Select(json, STJsonPathSelectMode.ItemOnly, null);
        }

        public static STJson Select(this STJson json, string str_path, STJsonPathCallBack callback)
        {
            return new STJsonPath(str_path).Select(json, STJsonPathSelectMode.ItemOnly, callback);
        }

        public static STJson Select(this STJson json, string str_path, STJsonPathSelectMode model)
        {
            return new STJsonPath(str_path).Select(json, model, null);
        }

        public static STJson Select(this STJson json, string str_path, STJsonPathSelectMode model, STJsonPathCallBack callback)
        {
            return new STJsonPath(str_path).Select(json, model, callback);
        }

        public static STJson Select(this STJson json, STJsonPath json_path)
        {
            return json_path.Select(json, STJsonPathSelectMode.ItemWithPath, null);
        }

        public static STJson Select(this STJson json, STJsonPath json_path, STJsonPathCallBack callBack)
        {
            return json_path.Select(json, STJsonPathSelectMode.ItemOnly, callBack);
        }

        public static STJson Select(this STJson json, STJsonPath json_path, STJsonPathSelectMode model)
        {
            return json_path.Select(json, model, null);
        }

        public static STJson Select(this STJson json, STJsonPath json_path, STJsonPathSelectMode model, STJsonPathCallBack callback)
        {
            return json_path.Select(json, model, callback);
        }

        // =============================================================================

        public static STJson SelectFirst(this STJson json, string str_path)
        {
            return new STJsonPath(str_path).SelectFirst(json);
        }

        public static STJson SelectFirst(this STJson json, string str_path, STJsonPathSelectMode model)
        {
            return new STJsonPath(str_path).SelectFirst(json, model);
        }

        public static STJson SelectLast(this STJson json, string str_path)
        {
            return new STJsonPath(str_path).SelectLast(json);
        }

        public static STJson SelectLast(this STJson json, string str_path, STJsonPathSelectMode model)
        {
            return new STJsonPath(str_path).SelectLast(json, model);
        }

        public static STJson SelectFirst(this STJson json, STJsonPath json_path)
        {
            return json_path.SelectFirst(json);
        }

        public static STJson SelectFirst(this STJson json, STJsonPath json_path, STJsonPathSelectMode model)
        {
            return json_path.SelectFirst(json, model);
        }

        public static STJson SelectLast(this STJson json, STJsonPath json_path)
        {
            return json_path.SelectLast(json);
        }

        public static STJson SelectLast(this STJson json, STJsonPath json_path, STJsonPathSelectMode model)
        {
            return json_path.SelectLast(json, model);
        }

        // ==================================================================================================

        public static STJson Group(this STJson json, params string[] arr_str_path)
        {
            return json.Group(ME.StrToPath(arr_str_path));
        }

        public static STJson Group(this STJson json, params STJsonPath[] arr_json_path)
        {
            if (json.ValueType != STJsonValueType.Array) {
                throw new STJsonAggregateException("Current STJson is not a Array.");
            }
            ME.CheckPathsName(arr_json_path);
            var json_ret = new STJson();
            var lst_dic = new List<Dictionary<object, STJson>>();
            foreach (var v in arr_json_path) {
                lst_dic.Add(new Dictionary<object, STJson>());
            }
            object obj_temp = null;
            foreach (var v in json) {
                if (v.ValueType != STJsonValueType.Object) {
                    continue;
                }
                for (int i = 0; i < arr_json_path.Length; i++) {
                    var dic = lst_dic[i];
                    var json_temp = arr_json_path[i].SelectFirst(v);
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
                        dic.Add(obj_temp, STJson.CreateArray());
                    }
                    dic[obj_temp].Append(v);
                }
            }
            for (int i = 0; i < lst_dic.Count; i++) {
                var json_group = STJson.CreateArray();
                foreach (var v in lst_dic[i]) {
                    json_group.Append(new STJson()
                        .SetItem("value", v.Key)
                        .SetItem("count", v.Value.Count)
                        .SetItem("items", v.Value)
                        );
                }
                json_ret.SetItem(arr_json_path[i].Name, json_group);
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Terms(this STJson json)
        {
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

        public static STJson Terms(this STJson json, params string[] arr_str_path)
        {
            return json.Terms(ME.StrToPath(arr_str_path));
        }

        public static STJson Terms(this STJson json, params STJsonPath[] arr_json_path)
        {
            if (json.ValueType != STJsonValueType.Array) {
                throw new STJsonAggregateException("Current STJson is not a Array.");
            }
            ME.CheckPathsName(arr_json_path);
            STJson json_ret = new STJson();
            List<object>[] lsts = new List<object>[arr_json_path.Length];
            for (int i = 0; i < arr_json_path.Length; i++) {
                lsts[i] = new List<object>();
            }
            foreach (var v in json) {
                for (int i = 0; i < arr_json_path.Length; i++) {
                    var temp = arr_json_path[i].SelectFirst(v);
                    if (temp == null) {
                        continue;
                    }
                    ME.GetTermsValue(temp, lsts[i]);
                }
            }
            Dictionary<object, int> dic = new Dictionary<object, int>();
            for (int i = 0; i < arr_json_path.Length; i++) {
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
                json_ret.SetItem(arr_json_path[i].Name, json_terms);
            }
            return json_ret;
        }

        private static void GetTermsValue(STJson json, List<object> lst_result)
        {
            switch (json.ValueType) {
                case STJsonValueType.Boolean:
                case STJsonValueType.String:
                case STJsonValueType.Long:
                case STJsonValueType.Double:
                case STJsonValueType.Datetime:
                    lst_result.Add(json.Value);
                    return;
                case STJsonValueType.Array:
                    foreach (var v in json) {
                        ME.GetTermsValue(v, lst_result);
                    }
                    return;
            }
        }

        // ==================================================================================================

        private struct MergeSortRange
        {
            public int Left;
            public int Right;
        }

        private static STJsonSortCallback BuildDefaultSortCallback(bool is_desc)
        {
            return (a, b) =>
            {
                int n_ret = 0;
                if (a.IsNullOrNullValue()) {
                    n_ret = -1;
                } else if (b.IsNullOrNullValue()) {
                    n_ret = 1;
                } else if (a.IsNumber && b.IsNumber) {
                    n_ret = Convert.ToDouble(a.Value) < Convert.ToDouble(b.Value) ? -1 : 1;
                } else if (a.ValueType != b.ValueType) {
                    n_ret = a.ValueType - b.ValueType;
                } else {
                    switch (a.ValueType) {
                        case STJsonValueType.Boolean:
                            n_ret = (bool)b.Value ? -1 : 1;
                            break;
                        case STJsonValueType.Datetime:
                            n_ret = (DateTime)a.Value < (DateTime)b.Value ? -1 : 1;
                            break;
                        case STJsonValueType.String:
                            n_ret = string.Compare(a.Value.ToString(), b.Value.ToString());
                            break;
                    }
                }
                return is_desc ? -n_ret : n_ret;
            };
        }

        private static STJsonSortCallback m_sort_callback_desc = ME.BuildDefaultSortCallback(true);
        private static STJsonSortCallback m_sort_callback_asc = ME.BuildDefaultSortCallback(false);

        public static STJson Sort(this STJson json, bool is_desc = false, bool is_new_instance = false)
        {
            return json.Sort(null, is_new_instance, is_desc ? m_sort_callback_desc : m_sort_callback_asc);
        }

        public static STJson Sort(this STJson json, string str_path, bool is_desc = false, bool is_new_instance = false)
        {
            return json.Sort(new STJsonPath(str_path), is_new_instance, is_desc ? m_sort_callback_desc : m_sort_callback_asc);
        }

        public static STJson Sort(this STJson json, STJsonPath json_path, bool is_desc = false, bool is_new_instance = false)
        {
            return json.Sort(json_path, is_new_instance, is_desc ? m_sort_callback_desc : m_sort_callback_asc);
        }

        public static STJson Sort(this STJson json, STJsonSortCallback callback)
        {
            return json.Sort(null, false, callback);
        }

        public static STJson Sort(this STJson json, bool is_new_instance, STJsonSortCallback callback)
        {
            return json.Sort(null, is_new_instance, callback);
        }

        private static STJson Sort(this STJson json, STJsonPath json_path, bool is_new_instance, STJsonSortCallback callback)
        {
            if (json.ValueType != STJsonValueType.Array) {
                throw new STJsonAggregateException("Current STJson is not a Array.");
            }
            if (!is_new_instance) {
                ME.MergeSortSplit(json, json_path, callback, new STJson[json.Count], 0, json.Count - 1);
                return json;
            }

            int[] arr_index_src = new int[json.Count];
            for (int i = 0, n_len = arr_index_src.Length; i < n_len; i++) {
                arr_index_src[i] = i;
            }
            ME.MergeSortSplit(json, json_path, callback, arr_index_src, new int[arr_index_src.Length], 0, json.Count - 1);

            STJson json_ret = STJson.CreateArray();
            foreach (var v in arr_index_src) {
                json_ret.Append(json[v]);
            }
            return json_ret;
        }

        private static void MergeSortSplit(STJson json, STJsonPath json_path, STJsonSortCallback callback, int[] arr_index_src, int[] arr_index_temp, int n_left, int n_right)
        {
            int n_mid = (n_left + n_right) >> 1;
            if (n_left >= n_right) {
                return;
            }
            var range_l = new MergeSortRange() { Left = n_left, Right = n_mid };
            var range_r = new MergeSortRange() { Left = n_mid + 1, Right = n_right };
            ME.MergeSortSplit(json, json_path, callback, arr_index_src, arr_index_temp, n_left, n_mid);
            ME.MergeSortSplit(json, json_path, callback, arr_index_src, arr_index_temp, n_mid + 1, n_right);
            ME.MergeSortMerge(json, json_path, callback, arr_index_src, arr_index_temp, range_l, range_r);
        }

        private static void MergeSortMerge(STJson json, STJsonPath json_path, STJsonSortCallback callback, int[] arr_index_src, int[] arr_index_temp, MergeSortRange range_l, MergeSortRange range_r)
        {
            int n_index = 0;
            int n_l_index = range_l.Left;
            int n_r_index = range_r.Left;
            if (json_path != null) {
                while (n_l_index <= range_l.Right && n_r_index <= range_r.Right) {
                    var json_a = json_path.SelectFirst(json[arr_index_src[n_l_index]]);
                    var json_b = json_path.SelectFirst(json[arr_index_src[n_r_index]]);
                    if (callback(json_a, json_b) > 0) {
                        arr_index_temp[n_index++] = arr_index_src[n_r_index++];
                    } else {
                        arr_index_temp[n_index++] = arr_index_src[n_l_index++];
                    }
                }
            } else {
                while (n_l_index <= range_l.Right && n_r_index <= range_r.Right) {
                    var json_a = json[arr_index_src[n_l_index]];
                    var json_b = json[arr_index_src[n_r_index]];
                    if (callback(json_a, json_b) > 0) {
                        arr_index_temp[n_index++] = arr_index_src[n_r_index++];
                    } else {
                        arr_index_temp[n_index++] = arr_index_src[n_l_index++];
                    }
                }
            }
            while (n_l_index <= range_l.Right) {
                arr_index_temp[n_index++] = arr_index_src[n_l_index++];
            }
            while (n_r_index <= range_r.Right) {
                arr_index_temp[n_index++] = arr_index_src[n_r_index++];
            }
            for (int i = range_l.Left, j = 0; i <= range_r.Right; i++, j++) {
                arr_index_src[i] = arr_index_temp[j];
            }
        }

        private static void MergeSortSplit(STJson json, STJsonPath json_path, STJsonSortCallback callback, STJson[] arr_json_temp, int n_left, int n_right)
        {
            int n_mid = (n_left + n_right) >> 1;
            if (n_left >= n_right) {
                return;
            }
            var range_l = new MergeSortRange() { Left = n_left, Right = n_mid };
            var range_r = new MergeSortRange() { Left = n_mid + 1, Right = n_right };
            ME.MergeSortSplit(json, json_path, callback, arr_json_temp, n_left, n_mid);
            ME.MergeSortSplit(json, json_path, callback, arr_json_temp, n_mid + 1, n_right);
            ME.MergeSortMerge(json, json_path, callback, arr_json_temp, range_l, range_r);
        }

        private static void MergeSortMerge(STJson json, STJsonPath json_path, STJsonSortCallback callback, STJson[] arr_json_temp, MergeSortRange range_l, MergeSortRange range_r)
        {
            int n_index = 0;
            int n_l_index = range_l.Left;
            int n_r_index = range_r.Left;
            if (json_path != null) {
                while (n_l_index <= range_l.Right && n_r_index <= range_r.Right) {
                    var json_a = json_path.SelectFirst(json[n_l_index]);
                    var json_b = json_path.SelectFirst(json[n_r_index]);
                    if (callback(json_a, json_b) > 0) {
                        arr_json_temp[n_index++] = json[n_r_index++];
                    } else {
                        arr_json_temp[n_index++] = json[n_l_index++];
                    }
                }
            } else {
                while (n_l_index <= range_l.Right && n_r_index <= range_r.Right) {
                    var json_a = json[n_l_index];
                    var json_b = json[n_r_index];
                    if (callback(json_a, json_b) > 0) {
                        arr_json_temp[n_index++] = json[n_r_index++];
                    } else {
                        arr_json_temp[n_index++] = json[n_l_index++];
                    }
                }
            }
            while (n_l_index <= range_l.Right) {
                arr_json_temp[n_index++] = json[n_l_index++];
            }
            while (n_r_index <= range_r.Right) {
                arr_json_temp[n_index++] = json[n_r_index++];
            }
            for (int i = range_l.Left, j = 0; i <= range_r.Right; i++, j++) {
                json[i] = arr_json_temp[j];
            }
        }

        /*
        private static void MergeSort(STJson json, int[] arr_index_src, int[] arr_index_temp, int n_left, int n_right)
        {
            int n_mid = (n_left + n_right) >> 1;
            if (n_left >= n_right) {
                return;
            }
            ME.MergeSort(json,arr_index_src, arr_index_temp, n_left, n_mid);
            ME.MergeSort(json,arr_index_src, arr_index_temp, n_mid + 1, n_right);
        }

        private static void MergeSortMerge(STJson json,System.Collections.IComparer aa)
        {
            
        }

        public static STJson Sort(this STJson json)
        {
            return json.Sort(false);
        }

        public static STJson Sort(this STJson json, bool isDesc)
        {
            return json.Sort(STJson.FromObject(new object[]{new {
                path = "",
                desc = isDesc
            } }));
        }

        public static STJson Sort(this STJson json, params object[] fields)
        {
            var jsonSort = STJson.CreateArray();
            for (int i = 0; i < fields.Length; i += 2) {
                var j = new STJson()
                    .SetItem("path", fields[i] == null ? null : fields[i].ToString())
                    .SetItem("desc", i + 1 < fields.Length ? Convert.ToBoolean(fields[i + 1]) : false);
                jsonSort.Append(j);
            }
            return json.Sort(jsonSort);
        }

        private static STJson Sort(this STJson json, STJson jsonSort)
        {
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

        private static MergeSortInfo[] GetMergeSortInfos(STJson json, STJson jsonSort)
        {
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
                                d_temp = Convert.ToDouble(item.GetValue<DateTime>().Ticks);
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

        private static void MergeSort(MergeSortInfo[] msis_temp, MergeSortInfo[] msis_src, int nKeysLen, int nLeft, int nRight)
        {
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

        private static void MergeSortMerge(MergeSortInfo[] msis_temp, MergeSortInfo[] msis_src, int nKeysLen, MergeSortRange rangeL, MergeSortRange rangeR)
        {
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
        */
        // ==================================================================================================

        public static STJson Min(this STJson json)
        {
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

        public static STJson Min(this STJson json, params string[] arr_str_path)
        {
            return json.Min(ME.StrToPath(arr_str_path));
        }

        public static STJson Min(this STJson json, params STJsonPath[] arr_json_path)
        {
            if (json.ValueType != STJsonValueType.Array) {
                throw new STJsonAggregateException("Current STJson is not a Array.");
            }
            ME.CheckPathsName(arr_json_path);
            List<int> lst_counter = new List<int>();
            List<double> lst_min_val = new List<double>();
            List<STJson> lst_min_json = new List<STJson>();
            foreach (var v in arr_json_path) {
                lst_counter.Add(0);
                lst_min_val.Add(double.MaxValue);
                lst_min_json.Add(STJson.CreateArray());
            }
            double d_val = 0;
            foreach (var v in json) {
                for (int i = 0; i < arr_json_path.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = arr_json_path[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long && json_item.ValueType != STJsonValueType.Double) {
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
            for (int i = 0; i < arr_json_path.Length; i++) {
                json_ret.SetItem(arr_json_path[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_min_val[i])
                    .SetItem("items", lst_min_json[i])
                    );
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Max(this STJson json)
        {
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

        public static STJson Max(this STJson json, params string[] arr_str_path)
        {
            return json.Max(ME.StrToPath(arr_str_path));
        }

        public static STJson Max(this STJson json, params STJsonPath[] arr_json_path)
        {
            if (json.ValueType != STJsonValueType.Array) {
                throw new STJsonAggregateException("Current STJson is not a Array.");
            }
            ME.CheckPathsName(arr_json_path);
            List<int> lst_counter = new List<int>();
            List<double> lst_max_val = new List<double>();
            List<STJson> lst_max_json = new List<STJson>();
            foreach (var v in arr_json_path) {
                lst_counter.Add(0);
                lst_max_val.Add(double.MinValue);
                lst_max_json.Add(STJson.CreateArray());
            }
            double d_val = 0;
            foreach (var v in json) {
                for (int i = 0; i < arr_json_path.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = arr_json_path[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long && json_item.ValueType != STJsonValueType.Double) {
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
            for (int i = 0; i < arr_json_path.Length; i++) {
                json_ret.SetItem(arr_json_path[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_max_val[i])
                    .SetItem("items", lst_max_json[i])
                    );
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Sum(this STJson json)
        {
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

        public static STJson Sum(this STJson json, params string[] arr_str_path)
        {
            return json.Sum(ME.StrToPath(arr_str_path));
        }

        public static STJson Sum(this STJson json, params STJsonPath[] arr_json_path)
        {
            if (json.ValueType != STJsonValueType.Array) {
                throw new STJsonAggregateException("Current STJson is not a Array.");
            }
            ME.CheckPathsName(arr_json_path);
            List<int> lst_counter = new List<int>();
            List<double> lst_sum = new List<double>();
            foreach (var v in arr_json_path) {
                lst_counter.Add(0);
                lst_sum.Add(0);
            }
            foreach (var v in json) {
                for (int i = 0; i < arr_json_path.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = arr_json_path[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long && json_item.ValueType != STJsonValueType.Double) {
                        continue;
                    }
                    lst_counter[i]++;
                    lst_sum[i] += json_item.GetValue<double>();
                }
            }
            STJson json_ret = new STJson();
            for (int i = 0; i < arr_json_path.Length; i++) {
                json_ret.SetItem(arr_json_path[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_sum[i])
                    );
            }
            return json_ret;
        }

        // ==================================================================================================

        public static STJson Avg(this STJson json)
        {
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

        public static STJson Avg(this STJson json, params string[] arr_str_path)
        {
            return json.Avg(ME.StrToPath(arr_str_path));
        }

        public static STJson Avg(this STJson json, params STJsonPath[] arr_json_path)
        {
            if (json.ValueType != STJsonValueType.Array) {
                throw new STJsonAggregateException("Current STJson is not a Array.");
            }
            ME.CheckPathsName(arr_json_path);
            List<int> lst_counter = new List<int>();
            List<double> lst_sum = new List<double>();
            foreach (var v in arr_json_path) {
                lst_counter.Add(0);
                lst_sum.Add(0);
            }
            foreach (var v in json) {
                for (int i = 0; i < arr_json_path.Length; i++) {
                    if (v.ValueType != STJsonValueType.Object) {
                        continue;
                    }
                    var json_item = arr_json_path[i].SelectFirst(v);// v[paths[i]];
                    if (json_item == null) {
                        continue;
                    }
                    if (json_item.ValueType != STJsonValueType.Long && json_item.ValueType != STJsonValueType.Double) {
                        continue;
                    }
                    lst_counter[i]++;
                    lst_sum[i] += json_item.GetValue<double>();
                }
            }
            STJson json_ret = new STJson();
            for (int i = 0; i < arr_json_path.Length; i++) {
                json_ret.SetItem(arr_json_path[i].Name, new STJson()
                    .SetItem("count", lst_counter[i])
                    .SetItem("value", lst_counter[i] == 0 ? 0 : lst_sum[i] / lst_counter[i])
                    );
            }
            return json_ret;
        }

        private static STJsonPath[] StrToPath(string[] arr_str_path)
        {
            var paths = new STJsonPath[arr_str_path.Length];
            for (int i = 0; i < arr_str_path.Length; i++) {
                paths[i] = new STJsonPath(arr_str_path[i], arr_str_path[i]);
            }
            return paths;
        }

        private static void CheckPathsName(STJsonPath[] arr_json_path)
        {
            var hs_name = new HashSet<string>();
            foreach (var v in arr_json_path) {
                if (!hs_name.Add(v.Name)) {
                    throw new STJsonAggregateException("The same path name {" + v.Name + "}");
                }
            }
        }
    }
}
