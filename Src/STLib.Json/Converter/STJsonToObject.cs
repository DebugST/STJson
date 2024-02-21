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

        public static T Get<T>(STJson json, STJsonSetting setting) {
            Type t = typeof(T);
            return (T)ME.GetObjectValue(json, t, setting);
        }

        public static void SetObject(STJson json, object obj, STJsonSetting setting) {
            var t = obj.GetType();
            var serilizaModel = STJsonSerilizaMode.All;
            if (!setting.IgnoreAttribute) {
#if NETSTANDARD
                var attr = t.GetCustomAttribute(m_type_attr_stjson);
                if (attr != null) {
                    serilizaModel = ((STJsonAttribute)attr).SerilizaMode;
                }
#else
                var attrs = t.GetCustomAttributes(m_type_attr_stjson, true);
                if (attrs != null && attrs.Length > 0) {
                    serilizaModel = ((STJsonAttribute)attrs[0]).SerilizaMode;
                }
#endif
            }
            var fps = FPInfo.GetFPInfo(t);// t.GetProperties();
            foreach (var p in fps) {
                if (json[p.KeyName] == null) continue;
                if (!p.CanSetValue) {
                    continue;
                }
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
                if (!p.CanSetValue) continue;
                object value = null;
                bool bProcessed = true;
                STJsonConverter converter = STJson.GetCustomConverter(p.KeyName);
                if (converter != null) {
                    value = converter.JsonToObject(t, json, ref bProcessed);
                }
                if (!bProcessed) {
                    value = ME.GetObjectValue(json[p.KeyName], p.Type, setting);
                }
                try {
                    p.SetValue(obj, value);
                } catch (Exception ex) {
                    throw new STJsonCastException("Can not set the [" + obj + "." + p.Name + "]", ex);
                }
            }
        }

        private static object GetObjectValue(STJson json, Type t, STJsonSetting setting) {
            object obj = null;
            bool bProcessed = true;
            STJsonConverter converter = STJson.GetConverter(t);
            if (converter != null) {
                obj = converter.JsonToObject(t, json, ref bProcessed);
                if (bProcessed) {
                    return obj;
                }
            }
            if (json == null || json.IsNullObject) {
                return null;
            }
            if (t == m_type_object) {
                if (json.Value != null) return json.Value;
                if (json.ValueType == STJsonValueType.Object) {
                    return ME.GetObjectValue(json, m_type_dic, setting);
                }
                if (json.ValueType == STJsonValueType.Array) {
                    return ME.GetObjectValue(json, m_type_arr, setting);
                }
            }
            if (t.IsEnum) {
                try {
                    switch (json.ValueType) {
                        case STJsonValueType.Long:
                            return Convert.ToInt32(json.Value);
                        case STJsonValueType.String:
                            return Enum.Parse(t, json.Value.ToString());
                        default:
                            throw new STJsonCastException("Can not convert the value {" + json.Value + "} to {" + t.FullName + "}");
                    }
                } catch {
                    throw new STJsonCastException("Can not convert [STJsonValueType." + json.ValueType + "]{" + json.Value + "} to [" + t.FullName + "]");
                }
            }
            switch (json.ValueType) {
                case STJsonValueType.String:
                case STJsonValueType.Long:
                case STJsonValueType.Double:
                case STJsonValueType.Boolean:
                    throw new STJsonCastException("Can not convert [STJsonValueType." + json.ValueType + "]{" + json.Value + "} to [" + t.FullName + "]");
            }
            if (t.IsArray) {
                var strFullName = t.FullName;
                int nDim = strFullName.Length - strFullName.LastIndexOf('[') - 1;
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
                ME.SetArray(arr, json, t.GetElementType(), nLens, nIndices, 0, setting);
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
                        idic.Add(ME.GetDicKey(type_key, j.Key), ME.GetObjectValue(j, type_value, setting));
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
                        ilst.Add(ME.GetObjectValue(j, type_item, setting));
                        //method.Invoke(obj, new object[] { ME.GetObjectValue(j, type_item, ignoreAttribute) });
                    }
                    return obj;
                }
#if NETSTANDARD
                var type_item_gen = t.GenericTypeArguments[0];
#else
                var type_item_gen = t.GetMethod("Add").GetParameters()[0].ParameterType;
#endif
                var method = t.GetMethod("Add");
                foreach (STJson j in json) {
                    method.Invoke(obj, new object[] { ME.GetObjectValue(j, type_item_gen, setting) });
                }
                return obj;
            }
            var serilizaModel = STJsonSerilizaMode.All;
            if (!setting.IgnoreAttribute) {
#if NETSTANDARD
                var attr = t.GetCustomAttribute(m_type_attr_stjson);
                if (attr != null) {
                    serilizaModel = ((STJsonAttribute)attr).SerilizaMode;
                }
#else
                var attrs = t.GetCustomAttributes(m_type_attr_stjson, true);
                if (attrs != null && attrs.Length > 0) {
                    serilizaModel = ((STJsonAttribute)attrs[0]).SerilizaMode;
                }
#endif
            }
            var fps = FPInfo.GetFPInfo(t);
            foreach (var p in fps) {
                if (json[p.KeyName] == null) continue;
                if (!p.CanSetValue) continue;
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
                object value = null;
                bProcessed = false;
                converter = p.Converter;
                if (converter == null) {
                    converter = STJson.GetCustomConverter(p.KeyName);
                }
                if (converter != null) {
                    bProcessed = true;
                    value = converter.JsonToObject(t, json[p.KeyName], ref bProcessed);
                }
                if (!bProcessed) {
                    value = ME.GetObjectValue(json[p.KeyName], p.Type, setting);
                }
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
            bool bProcessed = true;
            var convert = STJsonBuildInConverter.Get(t);
            if (convert != null) {
                var value = convert.JsonToObject(t, STJson.FromObject(strKey), ref bProcessed);
                if (bProcessed) {
                    return value;
                }
            }
            throw new STJsonCastException("Can not convert [string]{" + strKey + "} to [" + t.FullName + "]");
        }

        private static void SetArray(Array arr, STJson json, Type t, int[] nLens, int[] nIndices, int nLevel, STJsonSetting setting) {
            for (int i = 0; i < nLens[nLevel]; i++) {
                nIndices[nLevel] = i;
                if (nLevel == nLens.Length - 1) {
                    arr.SetValue(ME.GetObjectValue(json[nIndices[nLevel]], t, setting), nIndices);
                } else {
                    ME.SetArray(arr, json[i], t, nLens, nIndices, nLevel + 1, setting);
                }
            }
        }
    }
}

