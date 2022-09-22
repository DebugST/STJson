using System;
using System.Text;
using System.Collections;

using ME = STLib.Json.ObjectToString;
using System.Reflection;

namespace STLib.Json
{
    internal class ObjectToString
    {
        private static Type m_type_attr_stjson = typeof(STJsonAttribute);
        private static Type m_type_attr_stjson_property = typeof(STJsonPropertyAttribute);

        public static void Get(StringBuilder sb, object obj, bool ignoreAttribute) {
            if (obj == null) {
                sb.Append("null");
                return;
            }
            var t = obj.GetType();
            var pt_name = t.FullName;
            if (STJsonBasicDataType.Contains(pt_name)) {       // basic data type
                var tm = STJsonBasicDataType.Get(pt_name);
                switch (tm.ValueType) {
                    case STJsonValueType.Number:
                        sb.Append(obj.ToString());
                        return;
                    case STJsonValueType.String:
                        sb.Append("\"" + STJson.Escape(obj.ToString()) + "\"");
                        return;
                    case STJsonValueType.Boolean:
                        sb.Append(obj.ToString().ToLower());
                        return;
                    case STJsonValueType.Datetime:
                        sb.Append("\"" + ((DateTime)obj).ToString("O") + "\"");
                        return;
                }
            }
            if (t.IsEnum) {
                sb.Append(((int)obj).ToString());
                return;
            }

            if (t.IsArray) {
                Array arr = obj as Array;
                int nDim = t.FullName.Length - t.FullName.LastIndexOf('[') - 1;
                int[] nLens = new int[nDim];
                int[] nIndices = new int[nDim];
                for (int i = 0; i < nDim; i++) {
                    nLens[i] = arr.GetLength(i);
                }
                ME.GetArray(sb, arr, nLens, nIndices, 0, ignoreAttribute);
                return;
            }

            if (t.IsGenericType) {
                if (obj is IDictionary) {
                    var idic = (IDictionary)obj;
                    IEnumerator ie_keys = idic.Keys.GetEnumerator();
                    IEnumerator ie_values = idic.Values.GetEnumerator();
                    sb.Append('{');
                    while (ie_keys.MoveNext() && ie_values.MoveNext()) {
                        sb.Append("\"" + STJson.Escape(ie_keys.Current.ToString()) + "\"");
                        sb.Append(':');
                        ObjectToString.Get(sb, ie_values.Current, ignoreAttribute);
                        sb.Append(',');
                    }
                    ME.CheckEnd(sb, '}');
                    return;
                }

                if (obj is IEnumerable) {
                    IEnumerator ie = ((IEnumerable)obj).GetEnumerator();
                    sb.Append('[');
                    while (ie.MoveNext()) {
                        ObjectToString.Get(sb, ie.Current, ignoreAttribute);
                        sb.Append(',');
                    }
                    ME.CheckEnd(sb, ']');
                    return;
                }
            }

            var fps = FPInfo.GetFPInfo(t);
            if (fps.Count == 0) {
                sb.Append("\"" + STJson.Escape(obj.ToString()) + "\"");
                return;
            }
            var serilizaModel = STJsonSerilizaModel.All;
            if (!ignoreAttribute) {
#if NETSTANDARD
                var attr = t.GetCustomAttribute(m_type_attr_stjson);
                if (attr != null) {
                    serilizaModel = ((STJsonAttribute)attr).SerilizaModel;
                }
#else
                var attrs = t.GetCustomAttributes(m_type_attr_stjson, true);
                if (attrs != null && attrs.Length > 0) {
                    serilizaModel = ((STJsonAttribute)attrs[0]).SerilizaModel;
                }
#endif
            }
            sb.Append('{');
            foreach (var p in fps) {
                switch (serilizaModel) {
                    case STJsonSerilizaModel.All:
                        break;
                    case STJsonSerilizaModel.OnlyMarked:
                        if (p.GetCustomAttribute(m_type_attr_stjson_property) == null) {
                            continue;
                        }
                        break;
                    case STJsonSerilizaModel.ExcudeMarked:
                        if (p.GetCustomAttribute(m_type_attr_stjson_property) != null) {
                            continue;
                        }
                        break;
                }
                if (!p.CanGetValue) continue;
                sb.Append("\"" + STJson.Escape(p.Name) + "\"");
                sb.Append(':');
                ME.Get(sb, p.GetValue(obj), ignoreAttribute);
                sb.Append(',');
            }
            ME.CheckEnd(sb, '}');
        }

        private static void GetArray(StringBuilder sb, Array arr, int[] nLens, int[] nIndices, int nLevel, bool ignoreAttribute) {
            sb.Append('[');
            for (int i = 0; i < nLens[nLevel]; i++) {
                nIndices[nLevel] = i;
                if (nLevel == nLens.Length - 1) {
                    var obj = arr.GetValue(nIndices);
                    ME.Get(sb, obj, ignoreAttribute);
                } else {
                    ME.GetArray(sb, arr, nLens, nIndices, nLevel + 1, ignoreAttribute);
                }
                sb.Append(',');
            }
            ME.CheckEnd(sb, ']');
        }

        private static void CheckEnd(StringBuilder sb, char ch) {
            if (sb[sb.Length - 1] == ',') {
                sb[sb.Length - 1] = ch;
            } else {
                sb.Append(ch);
            }
        }
    }
}

