using System;
using System.Collections;
using System.Text;

using ME = STLib.Json.ObjectToString;

namespace STLib.Json
{
    internal class ObjectToString
    {
        private static Type m_type_attr_stjson = typeof(STJsonAttribute);

        public static void Get(StringBuilder sb, object obj, STJsonSetting setting) {
            if (obj == null) {
                sb.Append("null");
                return;
            }
            var t = obj.GetType();
            bool bProcessed = true;
            STJsonConverter converter = STJson.GetConverter(t);
            if (converter != null) {
                var str = converter.ObjectToString(t, obj, ref bProcessed);
                if (bProcessed) {
                    sb.Append(str);
                    return;
                }
            }
            if (t.IsEnum) {
                if (setting.EnumUseNumber) {
                    sb.Append((Convert.ToInt64(obj)).ToString());
                } else {
                    sb.Append('\"');
                    sb.Append(Convert.ToString(obj));
                    sb.Append('\"');
                }
                return;
            }

            if (t.IsArray) {
                Array arr = obj as Array;
                var strName = t.FullName;
                int nDim = strName.Length - strName.LastIndexOf('[') - 1;
                int[] nLens = new int[nDim];
                int[] nIndices = new int[nDim];
                for (int i = 0; i < nDim; i++) {
                    nLens[i] = arr.GetLength(i);
                }
                ME.GetArray(sb, arr, nLens, nIndices, 0, setting);
                return;
            }

            if (t.IsGenericType) {
                if (obj is IDictionary) {
                    var idic = (IDictionary)obj;
                    IEnumerator ie_keys = idic.Keys.GetEnumerator();
                    IEnumerator ie_values = idic.Values.GetEnumerator();
                    sb.Append('{');
                    while (ie_keys.MoveNext() && ie_values.MoveNext()) {
                        var strKey = ie_keys.Current.ToString();
                        sb.Append('\"');
                        sb.Append(STJson.Escape(strKey));
                        sb.Append("\":");
                        ME.Get(sb, ie_values.Current, setting);
                        sb.Append(',');
                    }
                    ME.CheckEnd(sb, '}');
                    return;
                }

                if (obj is IEnumerable) {
                    IEnumerator ie = ((IEnumerable)obj).GetEnumerator();
                    sb.Append('[');
                    while (ie.MoveNext()) {
                        ObjectToString.Get(sb, ie.Current, setting);
                        sb.Append(',');
                    }
                    ME.CheckEnd(sb, ']');
                    return;
                }
            }

            var fps = FPInfo.GetFPInfo(t);
            var serilizaModel = STJsonSerilizaMode.All;
            if (!setting.IgnoreAttribute) {
                //#if NETSTANDARD
                //                var attr = t.GetCustomAttribute(m_type_attr_stjson);
                //                if (attr != null) {
                //                    serilizaModel = ((STJsonAttribute)attr).SerilizaModel;
                //                }
                //#else
                var attrs = t.GetCustomAttributes(m_type_attr_stjson, true);
                if (attrs != null && attrs.Length > 0) {
                    serilizaModel = ((STJsonAttribute)attrs[0]).SerilizaMode;
                }
                //#endif
            }
            sb.Append('{');
            foreach (var p in fps) {
                switch (serilizaModel) {
                    case STJsonSerilizaMode.All:
                        break;
                    case STJsonSerilizaMode.Include:
                        if (p.PropertyAttribute == null) {
                            continue;
                        }
                        break;
                    case STJsonSerilizaMode.Exclude:
                        if (p.PropertyAttribute != null) {
                            continue;
                        }
                        break;
                }
                switch (setting.KyeMode) {
                    case STJsonSetting.KeyMode.Include:
                        if (!setting.KeyList.Contains(p.KeyName)) {
                            continue;
                        }
                        break;
                    case STJsonSetting.KeyMode.Exclude:
                        if (setting.KeyList.Contains(p.KeyName)) {
                            continue;
                        }
                        break;
                }
                bProcessed = true;
                converter = p.Converter;
                if (converter == null) {
                    converter = STJson.GetCustomConverter(p.KeyName);
                }
                if (converter != null) {
                    var vul = p.GetValue(obj);
                    if (vul == null && setting.IgnoreNullValue) {
                        continue;
                    }
                    var str = converter.ObjectToString(t, vul, ref bProcessed);
                    if (bProcessed) {
                        sb.Append('\"');
                        sb.Append(p.KeyName);
                        sb.Append("\":");
                        sb.Append(str);
                    }
                } else {
                    if (!p.CanGetValue) continue;
                    var vul = p.GetValue(obj);
                    if (vul == null && setting.IgnoreNullValue) {
                        continue;
                    }
                    sb.Append('\"');
                    sb.Append(p.KeyName);
                    sb.Append("\":");
                    ME.Get(sb, vul, setting);
                }
                sb.Append(',');
            }
            ME.CheckEnd(sb, '}');
        }

        private static void GetArray(StringBuilder sb, Array arr, int[] nLens, int[] nIndices, int nLevel, STJsonSetting setting) {
            sb.Append('[');
            for (int i = 0; i < nLens[nLevel]; i++) {
                nIndices[nLevel] = i;
                if (nLevel == nLens.Length - 1) {
                    var obj = arr.GetValue(nIndices);
                    ME.Get(sb, obj, setting);
                } else {
                    ME.GetArray(sb, arr, nLens, nIndices, nLevel + 1, setting);
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

