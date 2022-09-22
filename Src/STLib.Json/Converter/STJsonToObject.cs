using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using ME = STLib.Json.STJsonToObject;

namespace STLib.Json
{
    internal class STJsonToObject
    {
        private static Type m_type_object = typeof(object);
        private static Type m_type_arr = typeof(object[]);
        private static Type m_type_dic = typeof(Dictionary<string, object>);
        private static Type m_type_attr_stjson = typeof(STJsonAttribute);
        private static Type m_type_attr_stjson_property = typeof(STJsonPropertyAttribute);

        public static T Get<T>(STJson json, bool ignoreAttribute) {
            Type t = typeof(T);
            return (T)ME.GetObjectValue(json, t, ignoreAttribute);
        }

        public static void SetObject(STJson json, object obj, bool ignoreAttribute) {
            var t = obj.GetType();
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
            var fps = FPInfo.GetFPInfo(t);// t.GetProperties();
            foreach (var p in fps) {
                if (json[p.Name] == null) continue;
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
                if (!p.CanSetValue) continue;
                var value = ME.GetObjectValue(json[p.Name], p.Type, ignoreAttribute);
                try {
                    p.SetValue(obj, value);
                } catch (Exception ex) {
                    throw new STJsonCastException("Can not set the [" + obj + "." + p.Name + "]", ex);
                }
            }
        }

        private static object GetObjectValue(STJson json, Type t, bool ignoreAttribute) {
            object obj = null;
            string strFullName = t.FullName;
            if (json == null || json.IsNullObject) {
                if (STJsonBasicDataType.Contains(strFullName)) {
                    return STJsonBasicDataType.Get(strFullName).Converter(null);
                }
                return null;
            }
            if (STJsonBasicDataType.Contains(strFullName)) {
                return STJsonBasicDataType.Get(strFullName).Converter(json.Value);
            }
            if (t == m_type_object) {
                if (json.Value != null) return json.Value;
                if (json.ValueType == STJsonValueType.Object) {
                    return ME.GetObjectValue(json, m_type_dic, ignoreAttribute);
                }
                if (json.ValueType == STJsonValueType.Array) {
                    return ME.GetObjectValue(json, m_type_arr, ignoreAttribute);
                }
            }
            if (t.IsEnum) {
                try {
                    switch (json.ValueType) {
                        case STJsonValueType.Number:
                            return Convert.ToInt32(json.Value);
                        case STJsonValueType.String:
                            return Enum.Parse(t, json.Value.ToString());
                        default:
                            throw new Exception();
                    }
                } catch {
                    throw new STJsonCastException("Can not convert [STJsonValueType." + json.ValueType + "]{" + json.Value + "} to [" + t.FullName + "]");
                }
            }
            switch (json.ValueType) {
                case STJsonValueType.String:
                case STJsonValueType.Number:
                case STJsonValueType.Boolean:
                    throw new STJsonCastException("Can not convert [STJsonValueType." + json.ValueType + "]{" + json.Value + "} to [" + t.FullName + "]");
            }
            if (t.IsArray) {
                int nDim = t.FullName.Length - t.FullName.LastIndexOf('[') - 1;
                int[] nLens = new int[nDim];
                object[] objs_len = new object[nDim];
                int[] nIndices = new int[nDim];
                var jn = json;
                for (int i = 0; i < nDim; i++) {
                    nLens[i] = jn.Count;
                    objs_len[i] = jn.Count;
                    if (jn.Count == 0) {
                        break;
                    }
                    jn = jn[0];
                }
                obj = new object();
                Array arr = (Array)t.InvokeMember("Set", BindingFlags.CreateInstance, null, obj, objs_len);
                ME.SetArray(arr, json, t.GetElementType(), nLens, nIndices, 0, ignoreAttribute);
                return arr;
            }
            try {
                obj = Activator.CreateInstance(t);
            } catch (Exception ex) {
                throw new STJsonCastException("Can not create object [" + t.FullName + "].", ex);
            }
            if (t.IsGenericType) {
                if (obj is IDictionary) {
                    var idic = (IDictionary)obj;
#if NETSTANDARD
                    var type_key = t.GenericTypeArguments[0];
                    var type_value = t.GenericTypeArguments[1];
#else
                    var ps = t.GetMethod("Add").GetParameters();
                    var type_key = ps[0].ParameterType;
                    var type_value = ps[1].ParameterType;
#endif
                    //var method = t.GetMethod("Add");
                    foreach (STJson j in json) {
                        idic.Add(ME.GetDicKey(type_key, j.Key), ME.GetObjectValue(j, type_value, ignoreAttribute));
                        //method.Invoke(obj, new object[] { ME.GetDicKey(type_key, j.Key), ME.GetObjectValue(j, type_value, ignoreAttribute) });
                    }
                    return obj;
                }
                if (obj is IList) {
#if NETSTANDARD
                    var type_item = t.GenericTypeArguments[0];
#else
                    var type_item = t.GetMethod("Add").GetParameters()[0].ParameterType;
#endif
                    var ilst = (IList)obj;
                    //var method = t.GetMethod("Add");
                    foreach (STJson j in json) {
                        ilst.Add(ME.GetObjectValue(j, type_item, ignoreAttribute));
                        //method.Invoke(obj, new object[] { ME.GetObjectValue(j, type_item, ignoreAttribute) });
                    }
                    return obj;
                }
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
            var fps = FPInfo.GetFPInfo(t);
            foreach (var p in fps) {
                if (json[p.Name] == null) continue;
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
                if (!p.CanSetValue) continue;
                var value = ME.GetObjectValue(json[p.Name], p.Type, ignoreAttribute);
                try {
                    //mi.Invoke(obj, value);
                    p.SetValue(obj, value);
                } catch (Exception ex) {
                    throw new STJsonCastException("Can not set the [" + obj + "." + p.Name + "]", ex);
                }
            }
            return obj;
        }

        private static object GetDicKey(Type t, string strKey) {
            var strFullName = t.FullName;
            if (STJsonBasicDataType.Contains(strFullName)) {
                return STJsonBasicDataType.Get(strFullName).Converter(strKey);
            }
            throw new STJsonCastException("Can not convert [string]{" + strKey + "} to [" + t.FullName + "]");
        }

        private static void SetArray(Array arr, STJson json, Type t, int[] nLens, int[] nIndices, int nLevel, bool ignoreAttribute) {
            for (int i = 0; i < nLens[nLevel]; i++) {
                nIndices[nLevel] = i;
                if (nLevel == nLens.Length - 1) {
                    arr.SetValue(ME.GetObjectValue(json[nIndices[nLevel]], t, ignoreAttribute), nIndices);
                } else {
                    ME.SetArray(arr, json[i], t, nLens, nIndices, nLevel + 1, ignoreAttribute);
                }
            }
        }
    }
}

