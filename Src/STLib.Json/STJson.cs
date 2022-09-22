using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace STLib.Json
{
    public partial class STJson : IEnumerable
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
                    throw new Exception("Current STJson is not Object.");
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
                    throw new Exception("Current STJson is not Array.");
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
            return json;
        }

        public STJson SetItem(string key, bool value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return json;
        }

        public STJson SetItem(string key, double value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return json;
        }

        public STJson SetItem(string key, DateTime value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return json;
        }

        public STJson SetItem(string key, STJson value) {
            STJson json = this.SetKey(key);
            json.SetValue(value);
            return json;
        }


        public STJson Delete(string key) {
            if (this.ValueType != STJsonValueType.Object) {
                throw new Exception("Current STJson is not Object.");
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
            return this.Append(STJson.FromValue(value));
        }

        public STJson Append(double value) {
            return this.Append(STJson.FromValue(value));
        }

        public STJson Append(bool value) {
            return this.Append(STJson.FromValue(value));
        }

        public STJson Append(DateTime dateTime) {
            return this.Append(STJson.FromValue(dateTime));
        }

        public STJson Append(STJson json) {
            this.SetModel(STJsonValueType.Array);
            m_lst_values.Add(json);
            return json;
        }

        public STJson[] Append(params STJson[] jsons) {
            this.SetModel(STJsonValueType.Array);
            m_lst_values.AddRange(jsons);
            return jsons;
        }

        public STJson Insert(int nIndex, STJson json) {
            if (this.ValueType != STJsonValueType.Object) {
                throw new Exception("Current STJson is not Array.");
            }
            m_lst_values.Insert(nIndex, json);
            return this;
        }

        public STJson RemoveAt(int nIndex) {
            if (this.ValueType != STJsonValueType.Object) {
                throw new Exception("Current STJson is not Array.");
            }
            var json = m_lst_values[nIndex];
            m_lst_values.RemoveAt(nIndex);
            return json;
        }


        public string GetValue() {
            if (this.Value == null) {
                return "";
            }
            return this.Value.ToString();
        }

        public T GetValue<T>() {
            if (this.Value == null) {
                return default(T);
            }
            var t = typeof(T);
            if (STJsonBasicDataType.Contains(t.FullName)) {
                return (T)STJsonBasicDataType.Get(t.FullName).Converter(this.Value);
            }
            return (T)this.Value;
        }

        public void SetValue(DateTime dateTime) {
            this.Value = dateTime;
            this.SetModel(STJsonValueType.Datetime);
        }

        public void SetValue(string strText) {
            this.Value = strText;
            this.SetModel(STJsonValueType.String);
        }

        public void SetValue(bool boolean) {
            this.Value = boolean;
            this.SetModel(STJsonValueType.Boolean);
        }

        public void SetValue(double number) {
            this.Value = number;
            this.SetModel(STJsonValueType.Number);
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
                case STJsonValueType.Number:
                    sb.Append(this.Value.ToString());
                    break;
                case STJsonValueType.String:
                    if (this.Value == null) {
                        sb.Append("null");
                    } else {
                        sb.Append("\"" + STJson.Escape(this.Value.ToString()) + "\"");
                    }
                    break;
                case STJsonValueType.Boolean:
                    sb.Append(this.Value.ToString().ToLower());
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
                case STJsonValueType.Object:
                    if (m_dic_values == null) {
                        sb.Append("null");
                        break;
                    }
                    sb.Append('{');
                    foreach (var v in m_dic_values) {
                        sb.Append("\"" + STJson.Escape(v.Key.ToString()) + "\":");
                        v.Value.ToStringPrivate(sb);
                        sb.Append(',');
                    }
                    if (m_dic_values.Count == 0) {
                        sb.Append('}');
                    } else {
                        sb[sb.Length - 1] = '}';
                    }
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

        internal void SetModel(STJsonValueType valueType) {
            if (this.ValueType == valueType) return;
            this.ValueType = valueType;
            switch (valueType) {
                case STJsonValueType.String:
                case STJsonValueType.Boolean:
                case STJsonValueType.Datetime:
                case STJsonValueType.Number:
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
                case STJsonValueType.None:
                    this.Value = null;
                    m_dic_values = null;
                    m_lst_values = null;
                    break;
            }
        }
    }
}

