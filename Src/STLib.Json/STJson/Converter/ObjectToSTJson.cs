using System;
using System.Collections;

using ME = STLib.Json.ObjectToSTJson;

namespace STLib.Json
{
    internal class ObjectToSTJson
    {
        private static Type m_type_attr_stjson = typeof(STJsonAttribute);

        public static STJson Get(object obj, STJsonSetting setting)
        {
            if (obj == null) {
                return null;
            }
            if (obj is STJson) return (STJson)obj;
            var type = obj.GetType();
            STJson json = new STJson();
            bool b_processed = true;
            STJsonConverter converter = STJson.GetCustomConverter(type);
            if (converter == null) converter = STJson.GetConverter(type);
            if (converter != null) {
                var json_custom = converter.ObjectToJson(type, obj, ref b_processed);
                if (b_processed) {
                    return json_custom;
                }
            }
            if (type.IsEnum) {
                if (setting.EnumUseNumber) {
                    json.SetValue(Convert.ToInt64(obj));
                } else {
                    json.SetValue(Convert.ToString(obj));
                }
                return json;
            }

            if (type.IsArray) {
                Array arr = obj as Array;
                int nDim = type.FullName.Length - type.FullName.LastIndexOf('[') - 1;
                int[] nLens = new int[nDim];
                int[] nIndices = new int[nDim];
                for (int i = 0; i < nDim; i++) {
                    nLens[i] = arr.GetLength(i);
                }
                json.SetValue(GetArray(arr, nLens, nIndices, 0, setting));
                return json;
            }
            if (type.IsGenericType) {
                if (obj is IDictionary) {
                    var idic = (IDictionary)obj;
                    json.SetModel(STJsonValueType.Object);
                    //ICollection ic_keys = (ICollection)t.GetProperty("Keys").GetValue(obj, null);
                    //ICollection ic_values = (ICollection)t.GetProperty("Values").GetValue(obj, null);
                    IEnumerator ie_keys = idic.Keys.GetEnumerator();// ic_keys.GetEnumerator();
                    IEnumerator ie_values = idic.Values.GetEnumerator();// ic_values.GetEnumerator();
                    while (ie_keys.MoveNext() && ie_values.MoveNext()) {
                        var strKey = ie_keys.Current.ToString();
                        json.SetKey(strKey).SetValue(ME.Get(ie_values.Current, setting));
                    }
                    return json;
                }

                if (obj is IEnumerable) {
                    json.SetModel(STJsonValueType.Array);
                    //var method = t.GetMethod("GetEnumerator");
                    IEnumerator ie = ((IEnumerable)obj).GetEnumerator();// (IEnumerator)method.Invoke(obj, null);
                    while (ie.MoveNext()) {
                        json.Append(ME.Get(ie.Current, setting));
                    }
                    return json;
                }
            }
            json.SetModel(STJsonValueType.Object);
            var fps = FPInfo.GetFPInfo(type);
            var serilizaModel = STJsonSerializeMode.All;
            if (!setting.IgnoreAttribute) {
                //#if NETSTANDARD
                //                var attr = t.GetCustomAttribute(m_type_attr_stjson);
                //                if (attr != null) {
                //                    serilizaModel = ((STJsonAttribute)attr).SerilizaModel;
                //                }
                //#else
                var attrs = type.GetCustomAttributes(m_type_attr_stjson, true);
                if (attrs != null && attrs.Length > 0) {
                    serilizaModel = ((STJsonAttribute)attrs[0]).SerilizaMode;
                }
                //#endif
            }
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
                converter = p.Converter;
                if (converter == null) {
                    converter = STJson.GetCustomConverter(p.KeyName);
                }
                if (converter != null) {
                    b_processed = true;
                    var json_custom = converter.ObjectToJson(type, p.GetValue(obj), ref b_processed);
                    if (b_processed) {
                        return json.SetItem(p.KeyName, json_custom);
                    }
                }
                if (!p.CanGetValue) continue;
                json.SetKey(p.KeyName).SetValue(ME.Get(p.GetValue(obj), setting));
            }
            return json;
        }

        private static STJson GetArray(Array arr, int[] nLens, int[] nIndices, int nLevel, STJsonSetting setting)
        {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Array);
            for (int i = 0; i < nLens[nLevel]; i++) {
                nIndices[nLevel] = i;
                if (nLevel == nLens.Length - 1) {
                    var obj = arr.GetValue(nIndices);
                    json.Append(ME.Get(obj, setting));
                } else {
                    json.Append(GetArray(arr, nLens, nIndices, nLevel + 1, setting));
                }
            }
            return json;
        }
    }
}

