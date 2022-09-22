using System;
using System.Reflection;
using System.Collections.Generic;

namespace STLib.Json
{
    internal struct FPInfo
    {
        private FieldInfo m_f_info;
        private PropertyInfo m_p_info;

        public string Name {
            get {
                if (m_f_info != null) {
                    return m_f_info.Name;
                }
                return m_p_info.Name;
            }
        }

        public Type Type {
            get {
                if (m_f_info != null) {
                    return m_f_info.FieldType;
                }
                return m_p_info.PropertyType;
            }
        }

        public bool CanSetValue {
            get {
                if (m_f_info != null) return true;
                return m_p_info.GetSetMethod() != null;
            }
        }

        public bool CanGetValue {
            get {
                if (m_f_info != null) return true;
                return m_p_info.GetGetMethod() != null;
            }
        }

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
            List<FPInfo> lst = new List<FPInfo>();
            var fs = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var ps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var v in fs) {
                FPInfo fp = new FPInfo();
                fp.m_f_info = v;
                lst.Add(fp);
            }
            foreach (var v in ps) {
                FPInfo fp = new FPInfo();
                fp.m_p_info = v;
                lst.Add(fp);
            }
            return lst;
        }
    }
}

