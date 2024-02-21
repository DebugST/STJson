using System;
using System.Collections.Generic;
using System.Reflection;

namespace STLib.Json
{
    internal class FPInfo
    {
        private FieldInfo m_f_info;
        private PropertyInfo m_p_info;

        private static Type m_type_attr_property = typeof(STJsonPropertyAttribute);
        private static Type m_type_attr_converter = typeof(STJsonConverterAttribute);
        private static Dictionary<int, STJsonConverter> m_dic_type_map = new Dictionary<int, STJsonConverter>();
        private static Dictionary<int, List<FPInfo>> m_dic_fp_infos = new Dictionary<int, List<FPInfo>>();

        public string Name { get; private set; }
        public string KeyName { get; private set; }
        public STJsonConverter Converter { get; private set; }
        public STJsonPropertyAttribute PropertyAttribute { get; private set; }

        public Type Type { get; private set; }

        public bool CanSetValue { get; private set; }

        public bool CanGetValue { get; private set; }

        //public bool CanSetValue {
        //    get {
        //        if (m_f_info != null) return true;
        //        return m_p_info.GetSetMethod() != null;
        //    }
        //}

        //public bool CanGetValue {
        //    get {
        //        if (m_f_info != null) return true;
        //        return m_p_info.GetGetMethod() != null;
        //    }
        //}

        public object GetValue(object obj) {
            if (m_f_info != null) {
                return m_f_info.GetValue(obj);
            }
            return m_p_info.GetValue(obj, null);
        }

        public void SetValue(object obj, object value) {
            if (m_f_info != null) {
                m_f_info.SetValue(obj, value);
                return;
            }
            m_p_info.SetValue(obj, value, null);
        }

        public Attribute GetCustomAttribute(Type type) {
#if NETSTANDARD
            if (m_f_info != null) {
                return m_f_info.GetCustomAttribute(type);
            }
            return m_p_info.GetCustomAttribute(type);
#else
            if (m_f_info != null) {
                var f_attrs = m_f_info.GetCustomAttributes(type, true);
                if (f_attrs == null || f_attrs.Length == 0) {
                    return null;
                }
                return f_attrs[0] as Attribute;
            }
            var p_attrs = m_p_info.GetCustomAttributes(type, true);
            if (p_attrs == null || p_attrs.Length == 0) {
                return null;
            }
            return p_attrs[0] as Attribute;
#endif
        }

        public static List<FPInfo> GetFPInfo(Type type) {
            List<FPInfo> lst = null;
            int nCode = type.GetHashCode();
            if (m_dic_fp_infos.ContainsKey(nCode)) {
                return m_dic_fp_infos[nCode];
            } else {
                lst = new List<FPInfo>();
                m_dic_fp_infos.Add(nCode, lst);
            }
            var fs = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var ps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var v in fs) {
                FPInfo fp = new FPInfo();
                fp.m_f_info = v;
                fp.Name = v.Name;
                fp.Type = v.FieldType;
                fp.CanGetValue = true;
                fp.CanSetValue = true;
                lst.Add(fp);
            }
            foreach (var v in ps) {
                FPInfo fp = new FPInfo();
                fp.m_p_info = v;
                fp.Name = v.Name;
                fp.Type = v.PropertyType;
                fp.CanGetValue = v.GetGetMethod() != null;
                fp.CanSetValue = v.GetSetMethod() != null;
                if (fp.CanGetValue) {
                    fp.CanGetValue = v.GetGetMethod().GetParameters().Length == 0;
                }
                if (fp.CanSetValue) {
                    fp.CanSetValue = v.GetSetMethod().GetParameters().Length == 0;
                }
                lst.Add(fp);
            }
            for (int i = 0; i < lst.Count; i++) {
                var fp = lst[i];
                var attr = fp.GetCustomAttribute(m_type_attr_property);
                if (attr != null) {
                    fp.PropertyAttribute = attr as STJsonPropertyAttribute;
                }
                if (fp.PropertyAttribute != null && !string.IsNullOrEmpty(fp.PropertyAttribute.Name)) {
                    fp.KeyName = fp.PropertyAttribute.Name;
                } else {
                    fp.KeyName = fp.Name;
                }
                STJsonConverter converter = null;
                attr = fp.GetCustomAttribute(m_type_attr_converter);
                if (attr == null) {
                    continue;
                }
                nCode = attr.GetType().GetHashCode();
                if (m_dic_type_map.ContainsKey(nCode)) {
                    converter = m_dic_type_map[nCode];
                } else {
                    converter = (STJsonConverter)Activator.CreateInstance(((STJsonConverterAttribute)attr).Type);
                }
                fp.Converter = converter;
            }
            return lst;
        }
    }
}

