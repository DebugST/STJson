using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace STLib.Json
{
    public partial class STJson : IEnumerable, IEnumerable<STJson>, ICloneable
    {
        private STJson m_ref_json;

        public string Key { get; private set; }
        public object Value { get; private set; }
        public STJsonValueType ValueType { get; internal set; }

        [Obsolete("STJson.IsNullObject will be removed in the next version. Please use STJson.IsNullValue.")]
        public bool IsNullObject
        {
            get {
                //if (this.Value == null) {
                //    return m_dic_values == null && m_lst_values == null;
                //}
                //return false;
                return this.IsNullValue;
            }
        }

        public bool IsNullValue
        {
            get {
                return /*this.Value == null &&*/ this.ValueType == STJsonValueType.Undefined;
            }
        }

        public bool IsNumber
        {
            get { return this.ValueType == STJsonValueType.Long || this.ValueType == STJsonValueType.Double; }
        }

        public int Count
        {
            get {
                switch (this.ValueType) {
                    case STJsonValueType.Array:
                        return m_lst_values.Count;
                    case STJsonValueType.Object:
                        return m_dic_values.Count;
                }
                return 0;
            }
        }

        public STJson this[string key]
        {
            get {
                if (this.ValueType != STJsonValueType.Object) {
                    throw new STJsonException("Current STJson is not Object.");
                }
                if (m_dic_values.ContainsKey(key)) {
                    return m_dic_values[key];
                }
                return null;
            }
            set {
                if (this.ValueType != STJsonValueType.Object) {
                    throw new STJsonException("Current STJson is not Object.");
                }
                if (m_dic_values.ContainsKey(key)) {
                    m_dic_values[key].SetValue(value);
                } else {
                    m_dic_values.Add(key, value);
                }
            }
        }

        public STJson this[int index]
        {
            get {
                if (this.ValueType != STJsonValueType.Array) {
                    throw new STJsonException("Current STJson is not Array.");
                }
                return m_lst_values[index];
            }
            set {
                if (this.ValueType != STJsonValueType.Array) {
                    throw new STJsonException("Current STJson is not Array.");
                }
                m_lst_values[index] = value;
            }
        }

        private List<STJson> m_lst_values;
        private Dictionary<string, STJson> m_dic_values;

        public STJson()
        {
            this.ValueType = STJsonValueType.Undefined;
            m_lst_values = new List<STJson>();
            m_dic_values = new Dictionary<string, STJson>();
        }

        private STJson(string key) : this()
        {
            if (key == null) {
                key = "null";
            }
            this.Key = key;
        }

        public string[] GetKeys()
        {
            if (this.ValueType != STJsonValueType.Object) {
                return null;
            }
            int i = 0;
            string[] strs = new string[m_dic_values.Count];
            foreach (var v in m_dic_values.Keys) {
                strs[i++] = v;
            }
            return strs;
        }

        public STJson SetKey(string str_key)
        {
            this.SetModel(STJsonValueType.Object);
            if (str_key == null) {
                str_key = "null";
            }
            if (!m_dic_values.ContainsKey(str_key)) {
                m_dic_values.Add(str_key, new STJson(str_key));
            }
            return m_dic_values[str_key];
        }

        public STJson Remove(string str_key)
        {
            if (this.ValueType != STJsonValueType.Object) {
                throw new STJsonException("Current STJson is not Object.");
            }
            if (str_key == null) {
                str_key = "null";
            }
            STJson json = null;
            if (m_dic_values.ContainsKey(str_key)) {
                json = m_dic_values[str_key];
                m_dic_values.Remove(str_key);
            }
            return json;
        }

        public STJson Remove(int n_index)
        {
            if (this.ValueType != STJsonValueType.Object) {
                throw new STJsonException("Current STJson is not Array.");
            }
            var json = m_lst_values[n_index];
            m_lst_values.RemoveAt(n_index);
            return json;
        }

        [Obsolete("STJson.Delete(string) will be removed in the next version. Please use STJson.Remove(string).")]
        public STJson Delete(string key)
        {
            if (this.ValueType != STJsonValueType.Object) {
                throw new STJsonException("Current STJson is not Object.");
            }
            if (key == null) {
                key = "null";
            }
            STJson json = null;
            if (m_dic_values.ContainsKey(key)) {
                json = m_dic_values[key];
                m_dic_values.Remove(key);
            }
            return json;
        }

        [Obsolete("STJson.Delete(int) will be removed in the next version. Please use STJson.Remove(int).")]
        public STJson RemoveAt(int n_index)
        {
            if (this.ValueType != STJsonValueType.Object) {
                throw new STJsonException("Current STJson is not Array.");
            }
            var json = m_lst_values[n_index];
            m_lst_values.RemoveAt(n_index);
            return json;
        }

        public void Clear()
        {
            //if (m_dic_values != null) {
            m_dic_values.Clear();
            //}
            //if (m_lst_values != null) {
            m_lst_values.Clear();
            //}
            this.Value = null;
        }

        #region SetItem

        public STJson SetItem(string key, string value)
        {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, int value)
        {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, long value)
        {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, double value)
        {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, bool value)
        {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, DateTime value)
        {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, STJson json)
        {
            this.SetKey(key);//.SetValue(json);
            m_dic_values[key] = json;
            return this;
        }

        public STJson SetItem(string key, object obj)
        {
            STJson json = this.SetKey(key);
            json.SetValue(obj);
            return this;
        }

        #endregion SetItem

        #region SetValue

        public void SetValue(string value)
        {
            if (value == null) {
                this.SetValue(json: null);
                return;
            }
            this.Value = value;
            this.SetModel(STJsonValueType.String);
        }

        public void SetValue(int value)
        {
            this.Value = value;
            this.SetModel(STJsonValueType.Long);
        }

        public void SetValue(long value)
        {
            this.Value = value;
            this.SetModel(STJsonValueType.Long);
        }

        public void SetValue(float value)
        {
            this.Value = value;
            this.SetModel(STJsonValueType.Double);
        }

        public void SetValue(double value)
        {
            this.Value = value;
            this.SetModel(STJsonValueType.Double);
        }

        public void SetValue(bool value)
        {
            this.Value = value;
            this.SetModel(STJsonValueType.Boolean);
        }

        public void SetValue(DateTime value)
        {
            this.Value = value;
            this.SetModel(STJsonValueType.Datetime);
        }

        public void SetValue(STJson json)
        {
            this.SetValue(json, true);
        }

        private void SetValue(STJson json, bool is_make_ref)
        {
            if (json == null) {
                if (m_ref_json != null) {
                    m_ref_json.m_ref_json = null;
                    m_ref_json = null;
                }
                this.Value = null;
                this.SetModel(STJsonValueType.Undefined);
                //this.SetModel(STJsonValueType.Object);
                return;
            }
            this.Value = json.Value;
            this.ValueType = json.ValueType;
            m_dic_values = json.m_dic_values;
            m_lst_values = json.m_lst_values;
            if (is_make_ref) {
                json.m_ref_json = this;
                m_ref_json = json;
            }
        }

        public void SetValue(object obj)
        {
            if (obj is STJson) {
                this.SetValue((STJson)obj, true);
            } else {
                this.SetValue(STJson.FromObject(obj), false);
            }
        }

        #endregion SetValue

        #region GetValue

        public string GetValue()
        {
            if (this.Value == null) {
                return null;
            }
            return this.Value.ToString();
        }

        public T GetValue<T>()
        {
            if (this.Value == null) {
                return default(T);
            }
            var type = typeof(T);
            bool b_processed = true;
            var convert = STJsonBuildInConverter.Get(type);
            if (convert != null) {
                var value = convert.JsonToObject(type, this, ref b_processed);
                if (b_processed) {
                    return (T)value;
                }
            }
            return (T)this.Value;
        }

        public T GetValue<T>(T default_value)
        {
            try {
                return this.GetValue<T>();
            } catch {
                return default_value;
            }
        }

        public bool GetValue<T>(out T result)
        {
            try {
                result = this.GetValue<T>();
                return true;
            } catch {
                result = default(T);
                return false;
            }
        }

        #endregion GetValue


        #region Append

        public STJson Append(string value)
        {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(int value)
        {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(long value)
        {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(double value)
        {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(bool value)
        {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(DateTime value)
        {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(params object[] objs)
        {
            this.SetModel(STJsonValueType.Array);
            foreach (var v in objs) {
                this.Append(STJson.FromObject(v));
            }
            return this;
        }

        public STJson Append(STJson json)
        {
            m_lst_values.Add(json);
            this.SetModel(STJsonValueType.Array);
            return this;
        }

        #endregion Append

        #region Insert

        public STJson Insert(int n_index, string value)
        {
            return this.Insert(n_index, STJson.FromObject(value));
        }

        public STJson Insert(int n_index, int value)
        {
            return this.Insert(n_index, STJson.FromObject(value));
        }

        public STJson Insert(int n_index, long value)
        {
            return this.Insert(n_index, STJson.FromObject(value));
        }

        public STJson Insert(int n_index, double value)
        {
            return this.Insert(n_index, STJson.FromObject(value));
        }

        public STJson Insert(int n_index, bool value)
        {
            return this.Insert(n_index, STJson.FromObject(value));
        }

        public STJson Insert(int n_index, DateTime value)
        {
            return this.Insert(n_index, STJson.FromObject(value));
        }

        public STJson Insert(int n_index, object value)
        {
            return this.Insert(n_index, STJson.FromObject(value));
        }

        public STJson Insert(int n_index, STJson json)
        {
            if (this.ValueType != STJsonValueType.Object) {
                throw new STJsonException("Current STJson is not Array.");
            }
            m_lst_values.Insert(n_index, json);
            return this;
        }

        #endregion Insert

        public override string ToString()
        {
            return this.ToString(0);
        }

        public string ToString(int n_space_count)
        {
            StringWriter writer = new StringWriter();
            this.ToString(writer, n_space_count);
            return writer.ToString();
        }

        public void ToString(TextWriter writer, int n_space_count)
        {
            if (n_space_count < 0) {
                n_space_count = 0;
            }
            this.ToString(writer, 0, n_space_count);
        }

        internal void ToString(TextWriter writer, int n_level, int n_space_count)
        {
            var str_space_base = "".PadLeft((n_level) * n_space_count);
            var str_space_inc = "".PadLeft((n_level + 1) * n_space_count);
            int n_index = 0, n_len = 0;
            switch (this.ValueType) {
                case STJsonValueType.Undefined:
                    writer.Write("null");
                    break;
                case STJsonValueType.Long:
                case STJsonValueType.Double:
                    writer.Write(this.Value.ToString());
                    break;
                case STJsonValueType.String:
                    if (this.Value == null) {
                        writer.Write("null");
                    } else {
                        writer.Write('\"');
                        writer.Write(STJson.Escape(this.Value.ToString()));
                        writer.Write('\"');
                    }
                    break;
                case STJsonValueType.Boolean:
                    writer.Write(true.Equals(this.Value) ? "true" : "false");
                    break;
                case STJsonValueType.Datetime:
                    writer.Write('\"');
                    writer.Write(((DateTime)this.Value).ToString("O"));
                    writer.Write('\"');
                    break;
                case STJsonValueType.Array:
                    //if (m_lst_values == null) {
                    //    writer.Write("null");
                    //    break;
                    //}
                    if (m_lst_values.Count == 0) {
                        writer.Write("[]");
                        break;
                    }
                    writer.Write('[');
                    for (n_index = 0, n_len = m_lst_values.Count; n_index < n_len; n_index++) {
                        if (n_space_count != 0) {
                            writer.Write("\r\n" + str_space_inc);
                        }
                        if (m_lst_values[n_index] == null) {
                            writer.Write("null");
                        } else {
                            m_lst_values[n_index].ToString(writer, n_level + 1, n_space_count);
                        }
                        if (n_index != n_len - 1) {
                            writer.Write(',');
                        }
                    }
                    writer.Write(n_space_count == 0 ? "]" : ("\r\n" + str_space_base + "]"));
                    break;
                case STJsonValueType.Object:
                    //if (m_dic_values == null) {
                    //    writer.Write("null");
                    //    break;
                    //}
                    writer.Write('{');
                    if (m_dic_values.Count == 0) {
                        writer.Write("{}");
                        break;
                    }
                    n_index = 0; n_len = m_dic_values.Count;
                    foreach (var v in m_dic_values) {
                        writer.Write(n_space_count == 0 ? "\"" : ("\r\n" + str_space_inc + "\""));
                        writer.Write(STJson.Escape(v.Key.ToString()));
                        writer.Write(n_space_count == 0 ? "\":" : "\": ");
                        v.Value.ToString(writer, n_level + 1, n_space_count);
                        if (++n_index != n_len) {
                            writer.Write(',');
                        }
                    }
                    writer.Write(n_space_count == 0 ? "}" : ("\r\n" + str_space_base + "}"));
                    break;
                default:
                    writer.Write("{ERROR}");
                    break;
            }
        }

        //private void ToStringPrivate(StringBuilder sb)
        //{
        //    switch (this.ValueType) {
        //        case STJsonValueType.Long:
        //        case STJsonValueType.Double:
        //            sb.Append(this.Value.ToString());
        //            break;
        //        case STJsonValueType.String:
        //            if (this.Value == null) {
        //                sb.Append("null");
        //            } else {
        //                sb.Append('\"');
        //                sb.Append(STJson.Escape(this.Value.ToString()));
        //                sb.Append('\"');
        //            }
        //            break;
        //        case STJsonValueType.Boolean:
        //            sb.Append(true.Equals(this.Value) ? "true" : "false");
        //            break;
        //        case STJsonValueType.Array:
        //            if (m_lst_values == null) {
        //                sb.Append("null");
        //                break;
        //            }
        //            sb.Append('[');
        //            foreach (var v in m_lst_values) {
        //                if (v == null) {
        //                    sb.Append("null");
        //                } else {
        //                    v.ToStringPrivate(sb);
        //                }
        //                sb.Append(',');
        //            }
        //            if (m_lst_values.Count == 0) {
        //                sb.Append(']');
        //            } else {
        //                sb[sb.Length - 1] = ']';
        //            }
        //            break;
        //        case STJsonValueType.Datetime:
        //            sb.Append('\"');
        //            sb.Append(((DateTime)this.Value).ToString("O"));
        //            sb.Append('\"');
        //            break;
        //        case STJsonValueType.Object:
        //            if (m_dic_values == null) {
        //                sb.Append("null");
        //                break;
        //            }
        //            sb.Append('{');
        //            foreach (var v in m_dic_values) {
        //                sb.Append('\"');
        //                sb.Append(STJson.Escape(v.Key.ToString()));
        //                sb.Append("\":");
        //                v.Value.ToStringPrivate(sb);
        //                sb.Append(',');
        //            }
        //            if (m_dic_values.Count == 0) {
        //                sb.Append('}');
        //            } else {
        //                sb[sb.Length - 1] = '}';
        //            }
        //            break;
        //        default:
        //            sb.Append("{}");
        //            break;
        //    }
        //}

        public IEnumerator<STJson> GetEnumerator()
        {
            switch (this.ValueType) {
                case STJsonValueType.Array:
                    foreach (var v in m_lst_values) {
                        yield return v;
                    }
                    break;
                case STJsonValueType.Object:
                    foreach (var v in m_dic_values) {
                        yield return v.Value;
                    }
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public STJson Clone()
        {
            STJson json = new STJson();
            json.Value = this.Value;
            json.ValueType = this.ValueType;
            if (/*m_dic_values != null && */m_dic_values.Count > 0) {
                //if (json.m_dic_values == null) {
                //    json.m_dic_values = new Dictionary<string, STJson>();
                //}
                foreach (var v in m_dic_values) {
                    json.m_dic_values.Add(v.Key, v.Value.Clone());
                }
            }
            if (/*m_lst_values != null && */m_lst_values.Count > 0) {
                //if (json.m_lst_values == null) {
                //    json.m_lst_values = new List<STJson>();
                //}
                foreach (var v in m_lst_values) {
                    json.m_lst_values.Add(v.Clone());
                }
            }
            return json;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        
        internal void SetModel(STJsonValueType value_type)
        {
            if (this.ValueType == value_type) return;
            this.ValueType = value_type;
            switch (value_type) {
                case STJsonValueType.Long:
                case STJsonValueType.Double:
                case STJsonValueType.String:
                case STJsonValueType.Boolean:
                case STJsonValueType.Datetime:
                    m_dic_values.Clear();// = null;
                    m_lst_values.Clear();// = null;
                    break;
                case STJsonValueType.Array:
                    m_dic_values.Clear();// = null;
                    //if (m_lst_values == null) {
                    //    m_lst_values = new List<STJson>();
                    //}
                    break;
                case STJsonValueType.Object:
                    m_lst_values.Clear();// = null;
                    //if (m_dic_values == null) {
                    //    m_dic_values = new Dictionary<string, STJson>();
                    //}
                    break;
                default:// case STJsonValueType.Undefined:
                    this.Value = null;
                    m_dic_values.Clear();// = null;
                    m_lst_values.Clear();// = null;
                    break;
            }
            if (m_ref_json != null) {
                m_ref_json.Value = this.Value;
                m_ref_json.ValueType = this.ValueType;
            }
        }
    }
}

