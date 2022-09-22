using System;
using System.Collections;
using System.Reflection;
using System.Text;

using ME = STLib.Json.ObjectToSTJson;

namespace STLib.Json
{
    internal class ObjectToSTJson
    {
        private static Type m_type_attr_stjson = typeof(STJsonAttribute);
        private static Type m_type_attr_stjson_property = typeof(STJsonPropertyAttribute);

        public static STJson Get(object obj, bool ignoreAttribute) {
            if (obj == null) {
                return null;
            }
            STJson json = new STJson();
            var t = obj.GetType();
            var pt_name = t.FullName;
            if (STJsonBasicDataType.Contains(pt_name)) {
                var tm = STJsonBasicDataType.Get(pt_name);
                switch (tm.ValueType) {
                    case STJsonValueType.Number:
                        json.SetValue(Convert.ToDouble(obj));
                        return json;
                    case STJsonValueType.String:
                        json.SetValue(obj.ToString());
                        return json;
                    case STJsonValueType.Boolean:
                        json.SetValue((bool)obj);
                        return json;
                    case STJsonValueType.Datetime:
                        json.SetValue(((DateTime)obj).ToString("O"));
                        return json;
                }
            }
            if (t.IsEnum) {
                json.SetValue(Convert.ToDouble(obj));
                return json;
            }

            if (t.IsArray) {
                Array arr = obj as Array;
                int nDim = t.FullName.Length - t.FullName.LastIndexOf('[') - 1;
                int[] nLens = new int[nDim];
                int[] nIndices = new int[nDim];
                for (int i = 0; i < nDim; i++) {
                    nLens[i] = arr.GetLength(i);
                }
                json.SetValue(GetArray(arr, nLens, nIndices, 0, ignoreAttribute));
                return json;
            }
            if (t.IsGenericType) {
                if (obj is IDictionary) {
                    var idic = (IDictionary)obj;
                    json.SetModel(STJsonValueType.Object);
                    //ICollection ic_keys = (ICollection)t.GetProperty("Keys").GetValue(obj, null);
                    //ICollection ic_values = (ICollection)t.GetProperty("Values").GetValue(obj, null);
                    IEnumerator ie_keys = idic.Keys.GetEnumerator();// ic_keys.GetEnumerator();
                    IEnumerator ie_values = idic.Values.GetEnumerator();// ic_values.GetEnumerator();
                    while (ie_keys.MoveNext() && ie_values.MoveNext()) {
                        json.SetKey(ie_keys.Current.ToString())
                            .SetValue(ME.Get(ie_values.Current, ignoreAttribute));
                    }
                    return json;
                }

                if (obj is IEnumerable) {
                    json.SetModel(STJsonValueType.Array);
                    //var method = t.GetMethod("GetEnumerator");
                    IEnumerator ie = ((IEnumerable)obj).GetEnumerator();// (IEnumerator)method.Invoke(obj, null);
                    while (ie.MoveNext()) {
                        json.Append(ME.Get(ie.Current, ignoreAttribute));
                    }
                    return json;
                }
            }
            var fps = FPInfo.GetFPInfo(t);
            if (fps.Count == 0) {
                json.SetValue(obj.ToString());
                return json;
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
            json.SetModel(STJsonValueType.Object);
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
                json.SetKey(p.Name)
                    .SetValue(ObjectToSTJson.Get(p.GetValue(obj), ignoreAttribute));
            }
            return json;
        }

        private static STJson GetArray(Array arr, int[] nLens, int[] nIndices, int nLevel, bool ignoreAttribute) {
            STJson json = new STJson();
            json.SetModel(STJsonValueType.Array);
            for (int i = 0; i < nLens[nLevel]; i++) {
                nIndices[nLevel] = i;
                if (nLevel == nLens.Length - 1) {
                    var obj = arr.GetValue(nIndices);
                    json.Append(ObjectToSTJson.Get(obj, ignoreAttribute));
                } else {
                    json.Append(GetArray(arr, nLens, nIndices, nLevel + 1, ignoreAttribute));
                }
            }
            return json;
        }
    }
}

