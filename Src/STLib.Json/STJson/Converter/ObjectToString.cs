using System;
using System.IO;
using System.Collections;

using ME = STLib.Json.ObjectToString;

namespace STLib.Json
{
    internal class ObjectToString
    {
        private static Type m_type_json = typeof(STJson);
        private static Type m_type_attr_stjson = typeof(STJsonAttribute);

        public static void Get(TextWriter writer, int n_level, int n_space_count, object obj, STJsonSetting setting)
        {
            if (obj == null) {
                writer.Write("null");
                return;
            }
            var type = obj.GetType();
            if (type == m_type_json) {
                ((STJson)obj).ToString(writer, n_level, n_space_count);
                return;
            }
            bool bProcessed = true;
            STJsonConverter converter = STJson.GetCustomConverter(type);
            if (converter == null) converter = STJson.GetConverter(type);
            if (converter != null) {
                var str = converter.ObjectToString(type, obj, ref bProcessed);
                if (bProcessed) {
                    writer.Write(STJson.Format(str, n_space_count, "".PadLeft(n_level * n_space_count)));
                    return;
                }
            }
            if (type.IsEnum) {
                if (setting.EnumUseNumber) {
                    writer.Write((Convert.ToInt64(obj)).ToString());
                } else {
                    writer.Write('\"');
                    writer.Write(Convert.ToString(obj));
                    writer.Write('\"');
                }
                return;
            }

            if (type.IsArray) {
                Array arr = obj as Array;
                var str_name = type.FullName;
                int n_dim = str_name.Length - str_name.LastIndexOf('[') - 1;
                int[] nLens = new int[n_dim];
                int[] nIndices = new int[n_dim];
                for (int i = 0; i < n_dim; i++) {
                    nLens[i] = arr.GetLength(i);
                }
                ME.GetArray(writer, n_level + 1, n_space_count, arr, nLens, nIndices, 0, setting);
                return;
            }

            var n_counter = 0;
            var str_spance_base = "".PadLeft(n_level * n_space_count);
            var str_spance_inc = "".PadLeft((n_level + 1) * n_space_count);

            if (type.IsGenericType) {
                if (obj is IDictionary) {
                    var idic = (IDictionary)obj;
                    IEnumerator ie_keys = idic.Keys.GetEnumerator();
                    IEnumerator ie_values = idic.Values.GetEnumerator();
                    writer.Write('{');
                    while (ie_keys.MoveNext() && ie_values.MoveNext()) {
                        if (n_counter++ != 0) writer.Write(',');
                        var str_key = ie_keys.Current.ToString();
                        if (n_space_count == 0) {
                            writer.Write('\"');
                            writer.Write(STJson.Escape(str_key));
                            writer.Write("\":");
                        } else {
                            writer.Write("\r\n" + str_spance_inc + "\"");
                            writer.Write(STJson.Escape(str_key));
                            writer.Write("\": ");
                        }
                        ME.Get(writer, n_level + 1, n_space_count, ie_values.Current, setting);
                    }
                    if (n_counter == 0) {
                        writer.Write('}');
                    } else {
                        writer.Write(n_space_count == 0 ? "}" : ("\r\n" + str_spance_base + "}"));
                    }
                    return;
                }

                if (obj is IEnumerable) {
                    IEnumerator ie = ((IEnumerable)obj).GetEnumerator();
                    writer.Write('[');
                    while (ie.MoveNext()) {
                        if (n_counter++ != 0) writer.Write(',');
                        if (n_space_count != 0) {
                            writer.Write("\r\n" + str_spance_inc);
                        }
                        ObjectToString.Get(writer, n_level + 1, n_space_count, ie.Current, setting);
                    }
                    if (n_counter == 0) {
                        writer.Write(']');
                    } else {
                        writer.Write(n_space_count == 0 ? "]" : ("\r\n" + str_spance_base + "]"));
                    }
                    return;
                }
            }

            var fps = FPInfo.GetFPInfo(type);
            var serilizaModel = STJsonSerializeMode.All;
            if (!setting.IgnoreAttribute) {
                var attrs = type.GetCustomAttributes(m_type_attr_stjson, true);
                if (attrs != null && attrs.Length > 0) {
                    serilizaModel = ((STJsonAttribute)attrs[0]).SerilizaMode;
                }
            }
            n_counter = 0;
            writer.Write('{');
            foreach (var p in fps) {
                if (!p.CanGetValue) {
                    continue;
                }
                switch (serilizaModel) {
                    case STJsonSerializeMode.All:
                        break;
                    case STJsonSerializeMode.Include:
                        if (p.PropertyAttribute == null) {
                            continue;
                        }
                        break;
                    case STJsonSerializeMode.Exclude:
                        if (p.PropertyAttribute != null) {
                            continue;
                        }
                        break;
                }
                switch (setting.Mode) {
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
                var val = p.GetValue(obj);
                if (val == null && setting.IgnoreNullValue) {
                    continue;
                }
                if (n_counter++ != 0) writer.Write(',');
                if (n_space_count == 0) {
                    writer.Write('\"');
                    writer.Write(p.KeyName);
                    writer.Write("\":");
                } else {
                    writer.Write("\r\n" + str_spance_inc + "\"");
                    writer.Write(p.KeyName);
                    writer.Write("\": ");
                }

                bProcessed = true;
                converter = p.Converter;
                if (converter == null) {
                    converter = STJson.GetCustomConverter(p.KeyName);
                }
                if (converter != null) {
                    var str = converter.ObjectToString(type, val, ref bProcessed);
                    if (bProcessed) {
                        writer.Write(STJson.Format(str, n_space_count, str_spance_base));
                    }
                } else {
                    ME.Get(writer, n_level + 1, n_space_count, val, setting);
                }
            }
            if (n_counter == 0) {
                writer.Write('}');
            } else {
                writer.Write(n_space_count == 0 ? "}" : ("\r\n" + str_spance_base + "}"));
            }
        }

        private static void GetArray(TextWriter writer, int n_level_space, int n_space_count, Array arr, int[] arr_n_len, int[] arr_n_index, int n_level_arr, STJsonSetting setting)
        {
            int n_len = arr_n_len[n_level_arr];
            if (n_len == 0) {
                writer.Write("[]");
                return;
            }
            var str_spance_base = "".PadLeft(n_level_space * n_space_count);
            var str_spance_inc = "".PadLeft((n_level_space - 1) * n_space_count);
            writer.Write('[');
            for (int i = 0; i < n_len; i++) {
                if (i != 0) writer.Write(',');
                if (n_space_count != 0) {
                    writer.Write("\r\n" + str_spance_base);
                }
                arr_n_index[n_level_arr] = i;
                if (n_level_arr == arr_n_len.Length - 1) {
                    var obj = arr.GetValue(arr_n_index);
                    ME.Get(writer, n_level_space + 0, n_space_count, obj, setting);
                } else {
                    ME.GetArray(writer, n_level_space + 1, n_space_count, arr, arr_n_len, arr_n_index, n_level_arr + 1, setting);
                }
            }
            if (n_len == 0) {
                writer.Write(']');
            } else {
                writer.Write(n_space_count == 0 ? "]" : ("\r\n" + str_spance_inc + "]"));
            }
        }

        // ==========================================

        //public static void Get(StringBuilder sb, object obj, STJsonSetting setting)
        //{
        //    if (obj == null) {
        //        sb.Append("null");
        //        return;
        //    }
        //    var t = obj.GetType();
        //    if (t == m_type_json) {
        //        sb.Append(obj.ToString());
        //        return;
        //    }
        //    bool bProcessed = true;
        //    STJsonConverter converter = STJson.GetConverter(t);
        //    if (converter != null) {
        //        var str = converter.ObjectToString(t, obj, ref bProcessed);
        //        if (bProcessed) {
        //            sb.Append(str);
        //            return;
        //        }
        //    }
        //    if (t.IsEnum) {
        //        if (setting.EnumUseNumber) {
        //            sb.Append((Convert.ToInt64(obj)).ToString());
        //        } else {
        //            sb.Append('\"');
        //            sb.Append(Convert.ToString(obj));
        //            sb.Append('\"');
        //        }
        //        return;
        //    }

        //    if (t.IsArray) {
        //        Array arr = obj as Array;
        //        var strName = t.FullName;
        //        int nDim = strName.Length - strName.LastIndexOf('[') - 1;
        //        int[] nLens = new int[nDim];
        //        int[] nIndices = new int[nDim];
        //        for (int i = 0; i < nDim; i++) {
        //            nLens[i] = arr.GetLength(i);
        //        }
        //        ME.GetArray(sb, arr, nLens, nIndices, 0, setting);
        //        return;
        //    }

        //    if (t.IsGenericType) {
        //        if (obj is IDictionary) {
        //            var idic = (IDictionary)obj;
        //            IEnumerator ie_keys = idic.Keys.GetEnumerator();
        //            IEnumerator ie_values = idic.Values.GetEnumerator();
        //            sb.Append('{');
        //            while (ie_keys.MoveNext() && ie_values.MoveNext()) {
        //                var strKey = ie_keys.Current.ToString();
        //                sb.Append('\"');
        //                sb.Append(STJson.Escape(strKey));
        //                sb.Append("\":");
        //                ME.Get(sb, ie_values.Current, setting);
        //                sb.Append(',');
        //            }
        //            ME.CheckEnd(sb, '}');
        //            return;
        //        }

        //        if (obj is IEnumerable) {
        //            IEnumerator ie = ((IEnumerable)obj).GetEnumerator();
        //            sb.Append('[');
        //            while (ie.MoveNext()) {
        //                ObjectToString.Get(sb, ie.Current, setting);
        //                sb.Append(',');
        //            }
        //            ME.CheckEnd(sb, ']');
        //            return;
        //        }
        //    }

        //    var fps = FPInfo.GetFPInfo(t);
        //    var serilizaModel = STJsonSerilizaMode.All;
        //    if (!setting.IgnoreAttribute) {
        //        //#if NETSTANDARD
        //        //                var attr = t.GetCustomAttribute(m_type_attr_stjson);
        //        //                if (attr != null) {
        //        //                    serilizaModel = ((STJsonAttribute)attr).SerilizaModel;
        //        //                }
        //        //#else
        //        var attrs = t.GetCustomAttributes(m_type_attr_stjson, true);
        //        if (attrs != null && attrs.Length > 0) {
        //            serilizaModel = ((STJsonAttribute)attrs[0]).SerilizaMode;
        //        }
        //        //#endif
        //    }
        //    sb.Append('{');
        //    foreach (var p in fps) {
        //        if (!p.CanGetValue) {
        //            continue;
        //        }
        //        switch (serilizaModel) {
        //            case STJsonSerilizaMode.All:
        //                break;
        //            case STJsonSerilizaMode.Include:
        //                if (p.PropertyAttribute == null) {
        //                    continue;
        //                }
        //                break;
        //            case STJsonSerilizaMode.Exclude:
        //                if (p.PropertyAttribute != null) {
        //                    continue;
        //                }
        //                break;
        //        }
        //        switch (setting.Mode) {
        //            case STJsonSetting.KeyMode.Include:
        //                if (!setting.KeyList.Contains(p.KeyName)) {
        //                    continue;
        //                }
        //                break;
        //            case STJsonSetting.KeyMode.Exclude:
        //                if (setting.KeyList.Contains(p.KeyName)) {
        //                    continue;
        //                }
        //                break;
        //        }
        //        bProcessed = true;
        //        converter = p.Converter;
        //        if (converter == null) {
        //            converter = STJson.GetCustomConverter(p.KeyName);
        //        }
        //        if (converter != null) {
        //            var vul = p.GetValue(obj);
        //            if (vul == null && setting.IgnoreNullValue) {
        //                continue;
        //            }
        //            var str = converter.ObjectToString(t, vul, ref bProcessed);
        //            if (bProcessed) {
        //                sb.Append('\"');
        //                sb.Append(p.KeyName);
        //                sb.Append("\":");
        //                sb.Append(str);
        //            }
        //        } else {
        //            if (!p.CanGetValue) continue;
        //            var vul = p.GetValue(obj);
        //            if (vul == null && setting.IgnoreNullValue) {
        //                continue;
        //            }
        //            sb.Append('\"');
        //            sb.Append(p.KeyName);
        //            sb.Append("\":");
        //            ME.Get(sb, vul, setting);
        //        }
        //        sb.Append(',');
        //    }
        //    ME.CheckEnd(sb, '}');
        //}

        //private static void GetArray(StringBuilder sb, Array arr, int[] nLens, int[] nIndices, int nLevel, STJsonSetting setting)
        //{
        //    sb.Append('[');
        //    for (int i = 0; i < nLens[nLevel]; i++) {
        //        nIndices[nLevel] = i;
        //        if (nLevel == nLens.Length - 1) {
        //            var obj = arr.GetValue(nIndices);
        //            ME.Get(sb, obj, setting);
        //        } else {
        //            ME.GetArray(sb, arr, nLens, nIndices, nLevel + 1, setting);
        //        }
        //        sb.Append(',');
        //    }
        //    ME.CheckEnd(sb, ']');
        //}

        //private static void CheckEnd(StringBuilder sb, char ch)
        //{
        //    if (sb[sb.Length - 1] == ',') {
        //        sb[sb.Length - 1] = ch;
        //    } else {
        //        sb.Append(ch);
        //    }
        //}
    }
}

