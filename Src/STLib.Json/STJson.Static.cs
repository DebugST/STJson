using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace STLib.Json
{
    public partial class STJson
    {
        private static Regex m_reg_escape = new Regex(@"""|\\|\r|\n|\t");

        public static string Serialize(object obj) {
            StringBuilder sb = new StringBuilder();
            ObjectToString.Get(sb, obj, true);
            return sb.ToString();
        }

        public static string Serialize(object obj, int nSpaceCount) {
            StringBuilder sb = new StringBuilder();
            ObjectToString.Get(sb, obj, true);
            return STJson.Format(sb.ToString(), nSpaceCount);
        }

        public static string Serialize(object obj, bool ignoreAttribute) {
            StringBuilder sb = new StringBuilder();
            ObjectToString.Get(sb, obj, ignoreAttribute);
            return sb.ToString();
        }

        public static string Serialize(object obj, int nSpaceCount, bool ignoreAttribute) {
            StringBuilder sb = new StringBuilder();
            ObjectToString.Get(sb, obj, ignoreAttribute);
            return STJson.Format(sb.ToString(), nSpaceCount);
        }

        public static STJson Deserialize(string strJson) {
            return StringToSTJson.Get(strJson);
        }

        public static T Deserialize<T>(string strJson) {
            var json = StringToSTJson.Get(strJson);
            return STJsonToObject.Get<T>(json, true);
        }

        public static T Deserialize<T>(STJson stJson) {
            return STJsonToObject.Get<T>(stJson, true);
        }

        public static void Deserialize(string strJson, object obj) {
            var json = StringToSTJson.Get(strJson);
            STJsonToObject.SetObject(json, obj, true);
        }

        public static void Deserialize(STJson stJson, object obj) {
            STJsonToObject.SetObject(stJson, obj, true);
        }

        public static T Deserialize<T>(string strJson, bool ignoreAttribute) {
            var json = StringToSTJson.Get(strJson);
            return STJsonToObject.Get<T>(json, ignoreAttribute);
        }

        public static T Deserialize<T>(STJson stJson, bool ignoreAttribute) {
            return STJsonToObject.Get<T>(stJson, ignoreAttribute);
        }

        public static void Deserialize(string strJson, object obj, bool ignoreAttribute) {
            var json = StringToSTJson.Get(strJson);
            STJsonToObject.SetObject(json, obj, ignoreAttribute);
        }

        public static void Deserialize(STJson stJson, object obj, bool ignoreAttribute) {
            STJsonToObject.SetObject(stJson, obj, ignoreAttribute);
        }

        public static STJson CreateArray(params STJson[] jsons) {
            STJson json = new STJson();
            json.Append(jsons);
            return json;
        }

        public static STJson FromObject(object obj) {
            return ObjectToSTJson.Get(obj, false);
        }

        public static STJson FromObject(object obj, bool ignoreAttribute) {
            return ObjectToSTJson.Get(obj, ignoreAttribute);
        }

        public static STJson FromValue(string value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromValue(double value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromValue(bool value) {
            STJson json = new STJson();
            json.SetValue(value);
            return json;
        }

        public static STJson FromValue(DateTime dateTime) {
            STJson json = new STJson();
            json.SetValue(dateTime);
            return json;
        }

        public static string Format(string strJson) {
            return STJson.Format(strJson, 4);
        }

        public static string Format(string strJson, int nSpaceCount) {
            StringBuilder sb = new StringBuilder();
            int nLevel = 0;
            bool bString = false;
            bool bArray = false;
            Stack<bool> stack_is_array = new Stack<bool>();
            foreach (var c in strJson) {
                if (c == '"') {
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
                                sb.Append(c + "\r\n".PadRight(nLevel * nSpaceCount + 2));
                            }
                            continue;
                        case '{':
                        case '[':
                            bArray = c == '[';
                            stack_is_array.Push(bArray);
                            nLevel++;
                            sb.Append(c + "\r\n".PadRight(nLevel * nSpaceCount + 2));
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
            }
            return sb.ToString();
        }

        internal static string Escape(string strText) {
            return m_reg_escape.Replace(strText, (m) => {
                switch (m.Value) {
                    case "\r": return "\\r";
                    case "\n": return "\\r";
                    case "\t": return "\\t";
                    case "\"": return "\\\"";
                    case "\\": return "\\\\";
                }
                return string.Empty;
            });
        }
    }
}

