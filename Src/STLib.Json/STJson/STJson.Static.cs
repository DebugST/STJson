using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace STLib.Json
{
    public partial class STJson
    {
        internal static List<STJsonTypeMapInfo> m_lst_key_converter = new List<STJsonTypeMapInfo>();
        internal static List<STJsonTypeMapInfo> m_lst_type_converter = new List<STJsonTypeMapInfo>();

        internal static Dictionary<int, STJsonConverter> m_dic_key_converter = new Dictionary<int, STJsonConverter>();
        internal static Dictionary<int, STJsonConverter> m_dic_type_converter = new Dictionary<int, STJsonConverter>();

        public static void AddCustomConverter(Type type, STJsonConverter converter)
        {
            int n_code = type.GetHashCode();
            if (m_dic_type_converter.ContainsKey(n_code)) {
                m_dic_type_converter[n_code] = converter;
            } else {
                m_dic_type_converter.Add(n_code, converter);
            }
        }

        public static void AddCustomConverter(string str_key, STJsonConverter converter)
        {
            int nCode = str_key.GetHashCode();
            if (m_dic_key_converter.ContainsKey(nCode)) {
                m_dic_key_converter[nCode] = converter;
            } else {
                m_dic_key_converter.Add(nCode, converter);
            }
        }

        public static void RemoveCustomConverter(Type type)
        {
            int n_code = type.GetHashCode();
            if (m_dic_type_converter.ContainsKey(n_code)) {
                m_dic_type_converter.Remove(n_code);
            }
        }

        public static void RemoveCustomConverter(string str_key)
        {
            int n_code = str_key.GetHashCode();
            if (m_dic_key_converter.ContainsKey(n_code)) {
                m_dic_key_converter.Remove(n_code);
            }
        }

        public static STJsonConverter GetConverter(Type type)
        {
            int n_code = type.GetHashCode();
            if (m_dic_type_converter.ContainsKey(n_code)) {
                return m_dic_type_converter[n_code];
            } else {
                return STJsonBuildInConverter.Get(n_code);
            }
        }

        public static STJsonConverter GetCustomConverter(Type type)
        {
            int n_code = type.GetHashCode();
            if (m_dic_type_converter.ContainsKey(n_code)) {
                return m_dic_type_converter[n_code];
            }
            return null;
        }

        public static STJsonConverter GetCustomConverter(string str_key)
        {
            int n_code = str_key.GetHashCode();
            if (m_dic_key_converter.ContainsKey(n_code)) {
                return m_dic_key_converter[n_code];
            }
            return null;
        }

        public static string Serialize(object obj)
        {
            return STJson.Serialize(obj, 0, STJsonSetting.Default);
        }

        public static string Serialize(object obj, int n_space_count)
        {
            return STJson.Serialize(obj, n_space_count, STJsonSetting.Default);
        }

        public static string Serialize(object obj, STJsonSetting setting)
        {
            return STJson.Serialize(obj, 0, setting);
        }

        public static string Serialize(object obj, int n_space_count, STJsonSetting setting)
        {
            StringWriter writer = new StringWriter();
            STJson.Serialize(obj, writer, n_space_count, setting);
            return writer.ToString();
        }

        public static void Serialize(object obj, TextWriter writer)
        {
            STJson.Serialize(obj, writer, 0, STJsonSetting.Default);
        }

        public static void Serialize(object obj, TextWriter writer, int n_space_count)
        {
            STJson.Serialize(obj, writer, n_space_count, STJsonSetting.Default);
        }

        public static void Serialize(object obj, TextWriter writer, STJsonSetting setting)
        {
            STJson.Serialize(obj, writer, 0, setting);
        }

        public static void Serialize(object obj, TextWriter writer, int n_space_count, STJsonSetting setting)
        {
            if (n_space_count < 0) {
                n_space_count = 0;
            }
            ObjectToString.Get(writer, 0, n_space_count, obj, setting);
        }

        public static STJson Deserialize(string str_json)
        {
            return STJsonParser.Parse(str_json);
        }

        public static T Deserialize<T>(string str_json)
        {
            var json = STJsonParser.Parse(str_json);
            return STJsonToObject.Get<T>(json, STJsonSetting.Default);
        }

        public static T Deserialize<T>(STJson st_json)
        {
            return STJsonToObject.Get<T>(st_json, STJsonSetting.Default);
        }

        public static void Deserialize(string str_json, object obj)
        {
            var json = STJson.Deserialize(str_json);
            STJsonToObject.SetObject(json, obj, STJsonSetting.Default);
        }

        public static void Deserialize(STJson st_json, object obj)
        {
            STJsonToObject.SetObject(st_json, obj, STJsonSetting.Default);
        }

        public static T Deserialize<T>(string str_json, STJsonSetting setting)
        {
            var json = STJson.Deserialize(str_json);
            return STJsonToObject.Get<T>(json, setting);
        }

        public static T Deserialize<T>(STJson st_json, STJsonSetting setting)
        {
            return STJsonToObject.Get<T>(st_json, setting);
        }

        public static void Deserialize(string st_json, object obj, STJsonSetting setting)
        {
            var json = STJson.Deserialize(st_json);
            STJsonToObject.SetObject(json, obj, setting);
        }

        public static void Deserialize(STJson st_json, object obj, STJsonSetting setting)
        {
            STJsonToObject.SetObject(st_json, obj, setting);
        }

        public static STJson New()
        {
            return new STJson();
        }

        public static STJsonWriter Write(TextWriter writer)
        {
            return new STJsonWriter(writer, true, 0);
        }

        public static STJsonWriter Write(string str_file)
        {
            return new STJsonWriter(new StreamWriter(str_file, false, Encoding.UTF8), true, 0);
        }

        public static STJsonWriter Write(string str_file, int n_space_count)
        {
            return new STJsonWriter(new StreamWriter(str_file, false, Encoding.UTF8), true, n_space_count);
        }

        public static STJsonWriter Write(TextWriter writer, int n_space_count)
        {
            return new STJsonWriter(writer, true, n_space_count);
        }

        public static STJsonWriter Write(TextWriter writer, bool is_auto_close)
        {
            return new STJsonWriter(writer, is_auto_close, 0);
        }

        public static STJsonWriter Write(TextWriter writer, bool is_auto_close, int n_space_count)
        {
            return new STJsonWriter(writer, is_auto_close, n_space_count);
        }

        public static STJsonReader Read(string str_file)
        {
            return new STJsonReader(new StreamReader(str_file, Encoding.UTF8), true);
        }

        public static STJsonReader Read(TextReader reader)
        {
            return new STJsonReader(reader, true);
        }

        public static STJsonReader Read(TextReader reader, bool is_auto_close)
        {
            return new STJsonReader(reader, is_auto_close);
        }

        public static STJson CreateObject()
        {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Object);
            return json;
        }

        public static STJson CreateObject(STJsonWriterStartCallback callback)
        {
            StringWriter sw = new StringWriter();
            using (var writer = new STJsonWriter(sw)) {
                writer.StartWithObject(callback);
            }
            return STJsonParser.Parse(sw.ToString());
        }

        public static STJson CreateArray(params object[] objs)
        {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Array);
            foreach (var v in objs) {
                json.Append(v);
            }
            return json;
        }

        public static STJson CreateArray(STJsonWriterStartCallback callback)
        {
            StringWriter sw = new StringWriter();
            using (var writer = new STJsonWriter(sw)) {
                writer.StartWithArray(callback);
            }
            return STJsonParser.Parse(sw.ToString());
        }

        public static STJson FromObject(string value)
        {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(int value)
        {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(long value)
        {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(float value)
        {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(double value)
        {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(bool value)
        {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(DateTime value)
        {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromObject(object obj)
        {
            return ObjectToSTJson.Get(obj, STJsonSetting.Default);
        }

        public static STJson FromObject(object obj, STJsonSetting setting)
        {
            if (obj is STJson) return obj as STJson;
            return ObjectToSTJson.Get(obj, setting);
        }

        //public static string Format(string str_json)
        //{
        //    return STJson.Format(str_json, 4);
        //}

        //public static string Format(string str_json, int n_space_count)
        //{
        //    var json = STJson.Deserialize(str_json);
        //    return json.ToString(n_space_count);
        //}

        public static string Format(string str_json)
        {
            return STJson.Format(str_json, 4);
        }

        public static string Format(string str_json, int n_space_count)
        {
            return STJson.Format(str_json, 4, null);
        }

        public static string Format(string str_json, int n_space_count, string str_base_space)
        {
            str_base_space = "\r\n" + str_base_space;
            var str_space = string.Empty;
            char ch_last = '\0';
            StringBuilder sb = new StringBuilder();
            int n_level = 0;
            bool b_is_string = false;
            //bool b_is_array = false;
            //Stack<bool> stack_is_array = new Stack<bool>();
            //foreach (var c in str_json) {
            for (int i = 0; i < str_json.Length; i++) {
                var ch = str_json[i];
                if (ch == '"' && ch_last != '\\') {
                    b_is_string = !b_is_string;
                }
                if (!b_is_string) {
                    switch (ch) {
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            continue;
                        case ':':
                            sb.Append(": ");
                            continue;
                        case ',':
                            //if (b_is_array) {
                            //    sb.Append(", ");
                            //} else {
                            sb.Append("," + str_base_space);
                            sb.Append(/*"".PadRight(n_level * n_space_count)*/str_space);
                            //}
                            continue;
                        case '{':
                        case '[':
                            if (i + 1 < str_json.Length) {
                                switch (str_json[i + 1]) {
                                    case '}':
                                    case ']':
                                        sb.Append(ch);
                                        sb.Append(str_json[++i]);
                                        continue;
                                }
                            }
                            //b_is_array = ch == '[';
                            //stack_is_array.Push(b_is_array);
                            n_level++;
                            str_space = "".PadRight(n_level * n_space_count);
                            sb.Append(ch);
                            sb.Append(str_base_space);
                            sb.Append(/*"".PadRight(n_level * n_space_count)*/str_space);
                            continue;
                        case '}':
                        case ']':
                            //if (stack_is_array.Count == 0) {
                            //    b_is_array = false;
                            //} else {
                            //    stack_is_array.Pop();
                            //    b_is_array = stack_is_array.Count == 0 ? false : stack_is_array.First();
                            //}
                            n_level--;
                            str_space = "".PadRight(n_level * n_space_count);
                            sb.Append(str_base_space);
                            sb.Append(/*"".PadRight(n_level * n_space_count)*/str_space);
                            sb.Append(ch);
                            continue;
                    }
                }
                sb.Append(ch);
                ch_last = ch;
            }
            return sb.ToString();
        }

        internal static string Escape(string str_text)
        {
            if (string.IsNullOrEmpty(str_text)) return str_text;
            StringBuilder sb = new StringBuilder(str_text.Length);
            for (int i = 0; i < str_text.Length; i++) {
                var ch = str_text[i];
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
                switch (str_text[i]) {
                    case '\"': sb.Append("\\\""); continue;
                    case '\\': sb.Append("\\\\"); continue;
                    case '\b': sb.Append("\\b"); continue;
                    case '\f': sb.Append("\\f"); continue;
                    case '\n': sb.Append("\\n"); continue;
                    case '\r': sb.Append("\\r"); continue;
                    case '\t': sb.Append("\\t"); continue;
                    default:
                        sb.Append("\\u" + ((int)ch).ToString("X4"));
                        continue;
                }
            }
            if (sb.Length == str_text.Length) {
                return str_text;
            }
            return sb.ToString();
        }
    }
}

