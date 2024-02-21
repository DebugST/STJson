using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace STLib.Json
{
    public partial class STJson : IEnumerable, ICloneable
    {
        public string Key { get; private set; }
        public object Value { get; private set; }
        public bool IsNullObject {
            get {
                if (this.Value == null) {
                    return m_dic_values == null && m_lst_values == null;
                }
                return false;
            }
        }
        public bool IsNumber {
            get { return this.ValueType == STJsonValueType.Long || this.ValueType == STJsonValueType.Double; }
        }
        public STJsonValueType ValueType { get; internal set; }
        public int Count {
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

        public STJson this[string key] {
            get {
                if (this.ValueType != STJsonValueType.Object) {
                    throw new STJsonException("Current STJson is not Object.");
                }
                if (m_dic_values.ContainsKey(key)) {
                    return m_dic_values[key];
                }
                return null;
            }
        }

        public STJson this[int index] {
            get {
                if (this.ValueType != STJsonValueType.Array) {
                    throw new STJsonException("Current STJson is not Array.");
                }
                return m_lst_values[index];
            }
        }

        private List<STJson> m_lst_values;
        private Dictionary<string, STJson> m_dic_values;

        public STJson() { }

        private STJson(string key) {
            if (key == null) {
                key = "null";
            }
            this.Key = key;
        }

        public string[] GetKeys() {
            if (m_dic_values == null) {
                return null;
            }
            int i = 0;
            string[] strs = new string[m_dic_values.Count];
            foreach (var v in m_dic_values.Keys) {
                strs[i++] = v;
            }
            return strs;
        }

        public STJson SetKey(string key) {
            this.SetModel(STJsonValueType.Object);
            if (key == null) {
                key = "null";
            }
            if (!m_dic_values.ContainsKey(key)) {
                m_dic_values.Add(key, new STJson(key));
            }
            return m_dic_values[key];
        }

        public STJson SetItem(string key, string value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, int value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, long value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, double value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, bool value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, DateTime value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return this;
        }

        public STJson SetItem(string key, STJson json) {
            this.SetKey(key).SetValue(json);
            return this;
        }

        public STJson SetItem(string key, object obj) {
            STJson json = this.SetKey(key);
            json.SetValue(obj);
            return this;
        }


        public STJson Delete(string key) {
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


        public STJson Append(string value) {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(int value) {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(long value) {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(double value) {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(bool value) {
            return this.Append(STJson.FromObject(value));
        }

        public STJson Append(DateTime dateTime) {
            return this.Append(STJson.FromObject(dateTime));
        }

        public STJson Append(STJson json) {
            this.SetModel(STJsonValueType.Array);
            m_lst_values.Add(json);
            return this;
        }

        public STJson Append(params object[] objs) {
            this.SetModel(STJsonValueType.Array);
            foreach (var v in objs) {
                this.Append(STJson.FromObject(v));
            }
            return this;
        }

        public STJson Insert(int nIndex, string value) {
            return this.Insert(nIndex, STJson.FromObject(value));
        }

        public STJson Insert(int nIndex, int value) {
            return this.Insert(nIndex, STJson.FromObject(value));
        }

        public STJson Insert(int nIndex, long value) {
            return this.Insert(nIndex, STJson.FromObject(value));
        }

        public STJson Insert(int nIndex, double value) {
            return this.Insert(nIndex, STJson.FromObject(value));
        }

        public STJson Insert(int nIndex, bool value) {
            return this.Insert(nIndex, STJson.FromObject(value));
        }

        public STJson Insert(int nIndex, DateTime value) {
            return this.Insert(nIndex, STJson.FromObject(value));
        }

        public STJson Insert(int nIndex, object value) {
            return this.Insert(nIndex, STJson.FromObject(value));
        }

        public STJson Insert(int nIndex, STJson json) {
            if (this.ValueType != STJsonValueType.Object) {
                throw new STJsonException("Current STJson is not Array.");
            }
            m_lst_values.Insert(nIndex, json);
            return this;
        }

        public STJson RemoveAt(int nIndex) {
            if (this.ValueType != STJsonValueType.Object) {
                throw new STJsonException("Current STJson is not Array.");
            }
            var json = m_lst_values[nIndex];
            m_lst_values.RemoveAt(nIndex);
            return json;
        }

        public void Clear() {
            if (m_dic_values != null) {
                m_dic_values.Clear();
            }
            if (m_lst_values != null) {
                m_lst_values.Clear();
            }
            this.Value = null;
        }

        public void SetValue(string strText) {
            this.Value = strText;
            this.SetModel(STJsonValueType.String);
        }

        public void SetValue(int number) {
            this.Value = number;
            this.SetModel(STJsonValueType.Long);
        }

        public void SetValue(long number) {
            this.Value = number;
            this.SetModel(STJsonValueType.Long);
        }

        public void SetValue(float number) {
            this.Value = number;
            this.SetModel(STJsonValueType.Double);
        }


        public void SetValue(double number) {
            this.Value = number;
            this.SetModel(STJsonValueType.Double);
        }

        public void SetValue(bool boolean) {
            this.Value = boolean;
            this.SetModel(STJsonValueType.Boolean);
        }

        public void SetValue(DateTime dateTime) {
            this.Value = dateTime;
            this.SetModel(STJsonValueType.Datetime);
        }

        public void SetValue(STJson json) {
            if (json == null) {
                this.SetValue(strText: null);
                return;
            }
            this.Value = json.Value;
            this.ValueType = json.ValueType;
            m_dic_values = json.m_dic_values;
            m_lst_values = json.m_lst_values;
        }

        public void SetValue(object obj) {
            this.SetValue(STJson.FromObject(obj));
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            this.ToStringPrivate(sb);
            return sb.ToString();
        }

        public string ToString(int nSpaceCount) {
            string strJson = this.ToString();
            return STJson.Format(strJson, nSpaceCount);
        }

        private void ToStringPrivate(StringBuilder sb) {
            switch (this.ValueType) {
                case STJsonValueType.Long:
                case STJsonValueType.Double:
                    sb.Append(this.Value.ToString());
                    break;
                case STJsonValueType.String:
                    if (this.Value == null) {
                        sb.Append("null");
                    } else {
                        sb.Append('\"');
                        sb.Append(STJson.Escape(this.Value.ToString()));
                        sb.Append('\"');
                    }
                    break;
                case STJsonValueType.Boolean:
                    sb.Append(true.Equals(this.Value) ? "true" : "false");
                    break;
                case STJsonValueType.Array:
                    if (m_lst_values == null) {
                        sb.Append("null");
                        break;
                    }
                    sb.Append('[');
                    foreach (var v in m_lst_values) {
                        if (v == null) {
                            sb.Append("null");
                        } else {
                            v.ToStringPrivate(sb);
                        }
                        sb.Append(',');
                    }
                    if (m_lst_values.Count == 0) {
                        sb.Append(']');
                    } else {
                        sb[sb.Length - 1] = ']';
                    }
                    break;
                case STJsonValueType.Datetime:
                    sb.Append('\"');
                    sb.Append(((DateTime)this.Value).ToString("O"));
                    sb.Append('\"');
                    break;
                case STJsonValueType.Object:
                    if (m_dic_values == null) {
                        sb.Append("null");
                        break;
                    }
                    sb.Append('{');
                    foreach (var v in m_dic_values) {
                        sb.Append('\"');
                        sb.Append(STJson.Escape(v.Key.ToString()));
                        sb.Append("\":");
                        v.Value.ToStringPrivate(sb);
                        sb.Append(',');
                    }
                    if (m_dic_values.Count == 0) {
                        sb.Append('}');
                    } else {
                        sb[sb.Length - 1] = '}';
                    }
                    break;
                default:
                    sb.Append("{}");
                    break;
            }
        }

        public IEnumerator<STJson> GetEnumerator() {
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

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public STJson Clone() {
            STJson json = new STJson();
            json.Value = this.Value;
            json.ValueType = this.ValueType;
            if (m_dic_values != null && m_dic_values.Count > 0) {
                if (json.m_dic_values == null) {
                    json.m_dic_values = new Dictionary<string, STJson>();
                }
                foreach (var v in m_dic_values) {
                    json.m_dic_values.Add(v.Key, v.Value.Clone());
                }
            }
            if (m_lst_values != null && m_lst_values.Count > 0) {
                if (json.m_lst_values == null) {
                    json.m_lst_values = new List<STJson>();
                }
                foreach (var v in m_lst_values) {
                    json.m_lst_values.Add(v.Clone());
                }
            }
            return json;
        }

        object ICloneable.Clone() {
            return this.Clone();
        }

        internal void SetModel(STJsonValueType valueType) {
            if (this.ValueType == valueType) return;
            this.ValueType = valueType;
            switch (valueType) {
                case STJsonValueType.Long:
                case STJsonValueType.Double:
                case STJsonValueType.String:
                case STJsonValueType.Boolean:
                case STJsonValueType.Datetime:
                    m_dic_values = null;
                    m_lst_values = null;
                    break;
                case STJsonValueType.Array:
                    m_dic_values = null;
                    if (m_lst_values == null) {
                        m_lst_values = new List<STJson>();
                    }
                    break;
                case STJsonValueType.Object:
                    m_lst_values = null;
                    if (m_dic_values == null) {
                        m_dic_values = new Dictionary<string, STJson>();
                    }
                    break;
                default:// case STJsonValueType.Undefined:
                    this.Value = null;
                    m_dic_values = null;
                    m_lst_values = null;
                    break;
            }
        }

        internal void SetArray(List<STJson> lst) {
            this.SetModel(STJsonValueType.Array);
            m_lst_values = lst;
        }
    }
}

