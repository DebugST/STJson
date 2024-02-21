using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLib.Json
{
    public partial class STJson
    {
        internal static List<TypeMapInfo> m_lst_key_converter = new List<TypeMapInfo>();
        internal static List<TypeMapInfo> m_lst_type_converter = new List<TypeMapInfo>();

        internal static Dictionary<int, STJsonConverter> m_dic_key_converter = new Dictionary<int, STJsonConverter>();
        internal static Dictionary<int, STJsonConverter> m_dic_type_converter = new Dictionary<int, STJsonConverter>();

        public static void AddCustomConverter(Type t, STJsonConverter converter) {
            int nCode = t.GetHashCode();
            if (m_dic_type_converter.ContainsKey(nCode)) {
                m_dic_type_converter[nCode] = converter;
            } else {
                m_dic_type_converter.Add(nCode, converter);
            }
        }

        public static void AddCustomConverter(string strKey, STJsonConverter converter) {
            int nCode = strKey.GetHashCode();
            if (m_dic_key_converter.ContainsKey(nCode)) {
                m_dic_key_converter[nCode] = converter;
            } else {
                m_dic_key_converter.Add(nCode, converter);
            }
        }

        public static void RemoveCustomConverter(Type type) {
            int nCode = type.GetHashCode();
            if (m_dic_type_converter.ContainsKey(nCode)) {
                m_dic_type_converter.Remove(nCode);
            }
        }

        public static void RemoveCustomConverter(string strKey) {
            int nCode = strKey.GetHashCode();
            if (m_dic_key_converter.ContainsKey(nCode)) {
                m_dic_key_converter.Remove(nCode);
            }
        }

        public static STJsonConverter GetConverter(Type type) {
            int nCode = type.GetHashCode();
            if (m_dic_type_converter.ContainsKey(nCode)) {
                return m_dic_type_converter[nCode];
            } else {
                return STJsonBuildInConverter.Get(nCode);
            }
        }

        public static STJsonConverter GetCustomConverter(Type type) {
            int nCode = type.GetHashCode();
            if (m_dic_type_converter.ContainsKey(nCode)) {
                return m_dic_type_converter[nCode];
            }
            return null;
        }

        public static STJsonConverter GetCustomConverter(string strKey) {
            int nCode = strKey.GetHashCode();
            if (m_dic_key_converter.ContainsKey(nCode)) {
                return m_dic_key_converter[nCode];
            }
            return null;
        }

        public static string Serialize(object obj) {
            StringBuilder sb = new StringBuilder(512);
            ObjectToString.Get(sb, obj, STJsonSetting.Default);
            return sb.ToString();
        }

        public static string Serialize(object obj, int nSpaceCount) {
            StringBuilder sb = new StringBuilder(512);
            ObjectToString.Get(sb, obj, STJsonSetting.Default);
            return STJson.Format(sb.ToString(), nSpaceCount);
        }

        public static string Serialize(object obj, STJsonSetting setting) {
            StringBuilder sb = new StringBuilder(512);
            ObjectToString.Get(sb, obj, setting);
            return sb.ToString();
        }

        public static string Serialize(object obj, int nSpaceCount, STJsonSetting setting) {
            StringBuilder sb = new StringBuilder(512);
            ObjectToString.Get(sb, obj, setting);
            return STJson.Format(sb.ToString(), nSpaceCount);
        }

        public static STJson Deserialize(string strJson) {
            return StringToSTJson.Get(strJson);
        }

        public static T Deserialize<T>(string strJson) {
            var json = StringToSTJson.Get(strJson);
            return STJsonToObject.Get<T>(json, STJsonSetting.Default);
        }

        public static T Deserialize<T>(STJson stJson) {
            return STJsonToObject.Get<T>(stJson, STJsonSetting.Default);
        }

        public static void Deserialize(string strJson, object obj) {
            var json = StringToSTJson.Get(strJson);
            STJsonToObject.SetObject(json, obj, STJsonSetting.Default);
        }

        public static void Deserialize(STJson stJson, object obj) {
            STJsonToObject.SetObject(stJson, obj, STJsonSetting.Default);
        }

        public static T Deserialize<T>(string strJson, STJsonSetting setting) {
            var json = StringToSTJson.Get(strJson);
            return STJsonToObject.Get<T>(json, setting);
        }

        public static T Deserialize<T>(STJson stJson, STJsonSetting setting) {
            return STJsonToObject.Get<T>(stJson, setting);
        }

        public static void Deserialize(string strJson, object obj, STJsonSetting setting) {
            var json = StringToSTJson.Get(strJson);
            STJsonToObject.SetObject(json, obj, setting);
        }

        public static void Deserialize(STJson stJson, object obj, STJsonSetting setting) {
            STJsonToObject.SetObject(stJson, obj, setting);
        }

        public static STJson New() {
            return new STJson();
        }

        public static STJson CreateObject() {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Object);
            return json;
        }

        public static STJson CreateArray(params object[] objs) {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Array);
            foreach (var v in objs) {
                json.Append(v);
            }
            return json;
        }

        public static STJson FromObject(string value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(int value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(long value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(float value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(double value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(bool value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(DateTime dateTime) {
            STJson json = new STJson();
            json.SetValue(dateTime);
            return json;
        }

        public static STJson FromObject(object obj) {
            return ObjectToSTJson.Get(obj, STJsonSetting.Default);
        }

        public static STJson FromObject(object obj, STJsonSetting setting) {
            if (obj is STJson) return obj as STJson;
            return ObjectToSTJson.Get(obj, setting);
        }

        public static string Format(string strJson) {
            return STJson.Format(strJson, 4);
        }

        public static string Format(string strJson, int nSpaceCount) {
            char ch_last = '\0';
            StringBuilder sb = new StringBuilder();
            int nLevel = 0;
            bool bString = false;
            bool bArray = false;
            Stack<bool> stack_is_array = new Stack<bool>();
            foreach (var c in strJson) {
                if (c == '"' && ch_last != '\\') {
                    bString = !bString;
                }
                if (!bString) {
                    switch (c) {
                        case ':':
                            sb.Append(": ");
                            continue;
                        case ',':
                            if (bArray) {
                                sb.Append(", ");
                            } else {
                                sb.Append(c);
                                sb.Append("\r\n".PadRight(nLevel * nSpaceCount + 2)); // 2 -> \r\n
                            }
                            continue;
                        case '{':
                        case '[':
                            bArray = c == '[';
                            stack_is_array.Push(bArray);
                            nLevel++;
                            sb.Append(c);
                            sb.Append("\r\n".PadRight(nLevel * nSpaceCount + 2));
                            continue;
                        case '}':
                        case ']':
                            if (stack_is_array.Count == 0) {
                                bArray = false;
                            } else {
                                stack_is_array.Pop();
                                bArray = stack_is_array.Count == 0 ? false : stack_is_array.First();
                            }
                            nLevel--;
                            sb.Append("\r\n".PadRight(nLevel * nSpaceCount + 2) + c);
                            continue;
                    }
                }
                sb.Append(c);
                ch_last = c;
            }
            return sb.ToString();
        }

        internal static string Escape(string strText) {
            if (string.IsNullOrEmpty(strText)) return strText;
            StringBuilder sb = new StringBuilder(strText.Length);
            for (int i = 0; i < strText.Length; i++) {
                var ch = strText[i];
                if (0x5D <= ch && ch <= 0x10FFFF) {
                    sb.Append(ch);
                    continue;
                }
                if (0x23 <= ch && ch <= 0x5B) {
                    sb.Append(ch);
                    continue;
                }
                if (0x20 <= ch && ch <= 0x21) {
                    sb.Append(ch);
                    continue;
                }
                switch (strText[i]) {
                    case '\"': sb.Append("\\\""); continue;
                    case '\\': sb.Append("\\\\"); continue;
                    case '\b': sb.Append("\\b"); continue;
                    case '\f': sb.Append("\\f"); continue;
                    case '\n': sb.Append("\\n"); continue;
                    case '\r': sb.Append("\\r"); continue;
                    case '\t': sb.Append("\\t"); continue;
                    //case '\0': sb.Append("\\0"); continue;
                    //case '\a': sb.Append("\\a"); continue;
                    //case '\v': sb.Append("\\v"); continue;
                    //case '\x7F': sb.Append("\\u007F"); continue;
                    default:
                        //if (strText[i] < 32) {
                        //    sb.Append("\\x" + ((int)strText[i]).ToString("X4"));
                        //} else {
                        //    sb.Append(strText[i]);
                        //}
                        sb.Append("\\u" + ((int)ch).ToString("X4"));
                        continue;
                }
            }
            if (sb.Length == strText.Length) {
                return strText;
            }
            return sb.ToString();
        }
    }
}

